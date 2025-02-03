using System.ComponentModel.DataAnnotations;

// DTO
public class PaymentRequest
{
    [Required, MaxLength(34)]
    public required string DebtorAccount { get; set; }
    [Required, MaxLength(34)]
    public required string CreditorAccount { get; set; }
    [Required]
    public required decimal InstructedAmount { get; set; }
    [Required]
    public required string Currency { get; set; }
}
