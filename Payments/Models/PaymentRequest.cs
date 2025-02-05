public record PaymentRequest
{
    public required string DebtorAccount { get; init; }
    public required string CreditorAccount { get; init; }
    public required string InstructedAmount { get; init; }
    public required string Currency { get; init; }
}
