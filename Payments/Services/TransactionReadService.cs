public interface ITransactionReadService
{
    public List<TransactionRespone> GetCompletedTransactions(string iban);
}
public class TransactionReadService : ITransactionReadService
{
    private readonly ITransactionRepository _repository;
    public TransactionReadService(ITransactionRepository repository)
    {
        _repository = repository;
    }
    public List<TransactionRespone> GetCompletedTransactions(string iban)
    {
        return _repository.GetTransactions()
            .Where(t => t.IsCompleted && (t.DebtorAccount == iban || t.CreditorAccount == iban))
            .Select(t => new TransactionRespone
                    {
                    PaymentId = t.PaymentId.ToString(),
                    DebtorAccount = t.DebtorAccount,
                    CreditorAccount = t.CreditorAccount,
                    TransactionAmount = t.InstructedAmount,
                    Currency = t.Currency
                    })
        .ToList();
    }
}
