using System.Collections.Concurrent;

public interface ITransactionRepository
{
    public ConcurrentBag<PaymentTransaction> GetTransactions();
    public void AddTransaction(PaymentTransaction transaction);
}

public class TransactionRepository : ITransactionRepository
{
    public readonly ConcurrentBag<PaymentTransaction> _completedTransactions;

    public TransactionRepository()
    {
        _completedTransactions = new();
    }

    public ConcurrentBag<PaymentTransaction> GetTransactions()
    {
        return _completedTransactions;
    }

    public void AddTransaction(PaymentTransaction transaction)
    {
        _completedTransactions.Add(transaction);
    }
}
