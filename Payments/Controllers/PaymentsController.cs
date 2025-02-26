using Microsoft.AspNetCore.Mvc;

namespace Payments.Controllers;

[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly ITransactionWriteService _transactionWriteService;
    private readonly ITransactionReadService _transactionReadService;

    public PaymentsController(ITransactionWriteService transactionWriteService, ITransactionReadService transactionReadService)
    {
        _transactionWriteService = transactionWriteService;
        _transactionReadService = transactionReadService;
    }

    [HttpPost("/payments")]
    public async Task<IActionResult> InitiatePayment([FromHeader(Name = "Client-ID")] string clientId, [FromBody] PaymentRequest request)
    {
        if (string.IsNullOrEmpty(clientId))
            return BadRequest("Client-ID header is required.");

        var result = await _transactionWriteService.ProcessTransaction(clientId, request);

        return result.Status switch
        {
            TransactionStatus.Created => Created("/payments", result.Message),
            TransactionStatus.BadRequest => BadRequest(result.Message),
            TransactionStatus.Conflict => Conflict(result.Message),
            _ => StatusCode(500, "An unexpexted error occured")
        };
    }

    [HttpGet("/accounts/{iban}/transactions")]
    public IActionResult GetTransactions(string iban)
    {
        var transactions = _transactionReadService.GetCompletedTransactions(iban);
        return transactions.Any() ? Ok(transactions) : NoContent();
    }
}
