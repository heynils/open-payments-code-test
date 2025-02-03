using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Payments.Controllers;

[ApiController]
public class PaymentsController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _clientLocks = new();
    private static readonly ConcurrentDictionary<string, DateTime> _processingClients = new();
    private static readonly ConcurrentBag<PaymentTransaction> _completedTransactions = new();

    [HttpPost("/payments")]
    public async Task<IActionResult> InitiatePayment([FromHeader(Name = "Client-ID")] string clientId, [FromBody] PaymentRequest request)
    {
        if (string.IsNullOrEmpty(clientId))
            return BadRequest("Client-ID header is required.");

        var validationResult = ValidateRequest(request);
        if (validationResult != null)
            return validationResult;

        var clientLock = _clientLocks.GetOrAdd(clientId, _ => new SemaphoreSlim(1, 1));
        await clientLock.WaitAsync();

        try
        {
            if (_processingClients.TryGetValue(clientId, out var startTime) && (DateTime.UtcNow - startTime).TotalSeconds < 2)
                return Conflict("A payment is already in process for this client.");

            var paymentId = Guid.NewGuid();
            _processingClients[clientId] = DateTime.UtcNow;

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                _processingClients.TryRemove(clientId, out _);
                _completedTransactions.Add(new PaymentTransaction
                {
                    PaymentId = paymentId,
                    DebtorAccount = request.DebtorAccount,
                    CreditorAccount = request.CreditorAccount,
                    InstructedAmount = request.InstructedAmount,
                    Currency = request.Currency,
                    InitiatedAt = DateTime.UtcNow,
                });
            });

            return Created("/payments", paymentId);

        }
        finally
        {
            clientLock.Release();
        }
    }

    [HttpGet("/accounts/{iban}/transactions")]
    public IActionResult GetTransactions(string iban)
    {
        var transactions = _completedTransactions
            .Where(t => t.IsCompleted && (t.DebtorAccount == iban || t.CreditorAccount == iban))
            .Select(t => new TransactionRespone(
                        t.PaymentId.ToString(),
                        t.DebtorAccount,
                        t.CreditorAccount,
                        t.InstructedAmount,
                        t.Currency
                        ))
            .ToList();

        return transactions.Count > 0 ? Ok(transactions) : NoContent();
    }


    private IActionResult? ValidateRequest(PaymentRequest request)
    {
        // IBAN Validation
        const string ibanPattern = @"^[A-Za-z0-9]{1,34}$";
        if (string.IsNullOrEmpty(request.DebtorAccount) || !Regex.IsMatch(request.DebtorAccount, ibanPattern))
            return BadRequest("Debtor Account IBAN must be 1-34 alphanumeric characters");

        if (string.IsNullOrEmpty(request.CreditorAccount) || !Regex.IsMatch(request.CreditorAccount, ibanPattern))
            return BadRequest("Creditor Account IBAN must be 1-34 alphanumeric characters");

        // ISO 4217 Alpha 3 Validation
        const string currencyPattern = @"^[A-Z]{3}$";
        if (string.IsNullOrEmpty(request.Currency) || !Regex.IsMatch(request.Currency, currencyPattern))
            return BadRequest("Currency must be a 3-letter ISO 4217 code (e.g., USD, EUR)");


        const string amountPattern = @"^-?[0-9]{1,14}(\.[0-9]{1,3})?$";
        if (string.IsNullOrEmpty(request.InstructedAmount) || 
            !Regex.IsMatch(request.InstructedAmount, amountPattern))
        {
            return BadRequest("Instructed Amount must match pattern -?[0-9]{1,14}(.[0-9]{1,3})?");
        }

        return null;
    }
}
