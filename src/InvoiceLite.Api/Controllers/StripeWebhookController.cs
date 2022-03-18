using InvoiceLite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceLite.Api.Controllers;

// Stripe calls this without a JWT; requests are authenticated by the
// Stripe-Signature header instead.
[ApiController]
[AllowAnonymous]
[Route("api/webhooks/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly IPaymentService _payments;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(IPaymentService payments, ILogger<StripeWebhookController> logger)
    {
        _payments = payments;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();

        try
        {
            await _payments.HandleWebhookAsync(json, Request.Headers["Stripe-Signature"].ToString(), ct);
        }
        catch (Stripe.StripeException ex)
        {
            _logger.LogWarning(ex, "Rejected Stripe webhook");
            return BadRequest();
        }

        return Ok();
    }
}
