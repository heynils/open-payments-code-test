using System.Collections.Concurrent;
using System.Globalization;

public interface ITransactionWriteService
{
    public Task<TransactionResult> ProcessTransaction(string clientId, PaymentRequest paymentrequest);
}

public enum TransactionStatus
{
    Created,
    BadRequest,
    Conflict,
}

public class TransactionResult
{
    public required TransactionStatus Status { get; set; }
    public required string Message { get; set; }
}


public class TransactionWriteService : ITransactionWriteService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _clientLocks = new();
    private static readonly ConcurrentDictionary<string, DateTime> _processingClients = new();

    private readonly ITransactionRepository _repository;

    public TransactionWriteService(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<TransactionResult> ProcessTransaction(string clientId, PaymentRequest paymentRequest)
    {
        var clientLock = _clientLocks.GetOrAdd(clientId, _ => new SemaphoreSlim(1, 1));
        await clientLock.WaitAsync();

        try
        {
            if (_processingClients.TryGetValue(clientId, out var startTime) && (DateTime.UtcNow - startTime).TotalSeconds < 2)
                return new TransactionResult
                {
                    Status = TransactionStatus.Conflict,
                    Message = "A payment is already in process for this client."
                };

            _processingClients[clientId] = DateTime.UtcNow;

            var paymentId = Guid.NewGuid();
            var amount = decimal.Parse(paymentRequest.InstructedAmount, CultureInfo.InvariantCulture);

            // fire and forget
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                _processingClients.TryRemove(clientId, out _);
                _repository.AddTransaction(new PaymentTransaction
                {
                    PaymentId = paymentId,
                    DebtorAccount = paymentRequest.DebtorAccount,
                    CreditorAccount = paymentRequest.CreditorAccount,
                    InstructedAmount = amount,
                    Currency = paymentRequest.Currency,
                    InitiatedAt = DateTime.UtcNow,
                });
            });

            return new TransactionResult
            {
                Status = TransactionStatus.Created,
                Message = paymentId.ToString()
            };

        }
        catch (FormatException)
        {
            return new TransactionResult
            {
                Status = TransactionStatus.BadRequest,
                Message = "Invalid amount format.",
            };
        }
        finally
        {
            clientLock.Release();
        }

    }
}
