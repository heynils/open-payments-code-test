using System.ComponentModel.DataAnnotations;

public record PaymentRequest
{
    [Required(ErrorMessage= "Debtor Account is required")]
    [RegularExpression(@"^[A-Za-z0-9]{1,34}$", ErrorMessage = "Debtor Account IBAN must be 1-34 alphanumeric characters")]
    public required string DebtorAccount { get; init; }

    [Required(ErrorMessage= "Creditor Account is required")]
    [RegularExpression(@"^[A-Za-z0-9]{1,34}$", ErrorMessage = "Creditor Account IBAN must be 1-34 alphanumeric characters")]
    public required string CreditorAccount { get; init; }

    [Required(ErrorMessage= "Instructed Amount is required")]
    [RegularExpression(@"^-?[0-9]{1,14}(\.[0-9]{1,3})?$", ErrorMessage = "Instructed Amount must match pattern -?[0-9]{1,14}(.[0-9]{1,3})?")]
    public required string InstructedAmount { get; init; }

    [Required(ErrorMessage= "Currency is required")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be a 3-letter ISO 4217 code (e.g., USD, EUR)")]
    public required string Currency { get; init; }
}
