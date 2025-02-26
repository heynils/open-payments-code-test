using System.Collections.Concurrent;

public interface ITransactionRepository
{
    public ConcurrentBag<PaymentTransaction> GetTransactions();
}

public class TransactionRepository : ITransactionRepository
{
    public readonly ConcurrentBag<PaymentTransaction> _completedTransactions;

    public TransactionRepository()
    {
        _completedTransactions = new();
    }

    ConcurrentBag<PaymentTransaction> ITransactionRepository.GetTransactions()
    {
        return _completedTransactions;
    }
}
