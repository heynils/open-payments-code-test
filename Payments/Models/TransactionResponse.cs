public record TransactionRespone
{
    public required string PaymentId { get; init; }
    public required string DebtorAccount { get; init; }
    public required string CreditorAccount { get; init; }
    public required decimal TransactionAmount { get; init; }
    public required string Currency { get; init; }
}
