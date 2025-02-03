using System.ComponentModel.DataAnnotations;

public class Payment
{
    [Key]
    public Guid PaymentId { get; set; } = Guid.NewGuid();
    // regex?
    public required string DebtorAccount { get; set; } 
    // regex?
    public required string CreditorAccount { get; set; } 
    // regex?
    public required decimal InstructedAmount { get; set; } 
    // ISO domain object for currency?
    public required string Currency { get; set; }
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; } = false;
}
