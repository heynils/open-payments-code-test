using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;

namespace Payments.Controllers;

[ApiController]
[Route("/payments")]
public class PaymentsController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, Payment> _payments = new();
    private static readonly ConcurrentDictionary<Guid, Lock> _clientLocks = new();

    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(ILogger<PaymentsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> InitiatePayment([FromHeader(Name = "Client-ID")] Guid clientId, [FromBody] PaymentRequest request)
    {
        var clientLock = _clientLocks.GetOrAdd(clientId, _ => new Lock());

        lock(clientLock)
        {
            var existingPayment = _payments.Values.Any(p => !p.IsCompleted && p.DebtorAccount == request.DebtorAccount);
            if (existingPayment)
            {
                return Conflict("A payment is already in process for this Client-ID");
            }
        }

        var payment = new Payment
        {
            DebtorAccount = request.DebtorAccount,
            CreditorAccount = request.CreditorAccount,
            InstructedAmount = request.InstructedAmount,
            Currency = request.Currency
        };

        _payments[payment.PaymentId] = payment;

        await CompletePaymentAsync(payment.PaymentId);

        return Created("/payments", payment.PaymentId);
    }


    private async Task CompletePaymentAsync(Guid paymentId)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        if (_payments.TryGetValue(paymentId, out var payment))
        {
            payment.IsCompleted = true;
        }
    }
}
