public class PaymentTransaction
{
    public required Guid PaymentId { get; set; }
    public required string DebtorAccount { get; set; } 
    public required string CreditorAccount { get; set; } 
    public required string InstructedAmount { get; set; } 
    public required string Currency { get; set; }
    public required DateTime InitiatedAt { get; set; }
    public bool IsCompleted => DateTime.UtcNow >= InitiatedAt.AddSeconds(2);
}
