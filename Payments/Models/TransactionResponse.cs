public record TransactionRespone(
        string PaymentId,
        string DebtorAccount,
        string CreditorAccount,
        string TransactionAmount,
        string Currency
);

