public record TransactionRespone
{
    public required string PaymentId { get; set; }
    public required string DebtorAccount { get; set; }
    public required string CreditorAccount { get; set; }
    public required decimal TransactionAmount { get; set; }
    public required string Currency { get; set; }
}
