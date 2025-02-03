public record TransactionRespone(
        string PaymentId,
        string DebtorAccount,
        string CreditorAccount,
        decimal TransactionAmount,
        string Currency
);

