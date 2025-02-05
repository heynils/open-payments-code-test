public record PaymentTransaction
{
    public required Guid PaymentId { get; init; }
    public required string DebtorAccount { get; init; }
    public required string CreditorAccount { get; init; }
    public required decimal InstructedAmount { get; init; }
    public required string Currency { get; init; }
    public required DateTime InitiatedAt { get; init; }
    public bool IsCompleted => DateTime.UtcNow >= InitiatedAt.AddSeconds(2);
}
