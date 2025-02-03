public class Payment
{
    public Guid PaymentId { get; }
    // regex?
    public required string DebtorAccount { get; set; } 
    // regex?
    public required string CreditorAccount { get; set; } 
    // regex?
    public required decimal InstructedAmount { get; set; } 
    // ISO domain object for currency?
    public required string Currency { get; set; }
    public DateTime InitiatedAt { get; set; }
    public bool IsCompleted => DateTime.UtcNow >= InitiatedAt.AddSeconds(2);
}
