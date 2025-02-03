public record PaymentRequest
{
    public required string DebtorAccount { get; set; }
    public required string CreditorAccount { get; set; }
    public required string InstructedAmount { get; set; }
    public required string Currency { get; set; }
}
