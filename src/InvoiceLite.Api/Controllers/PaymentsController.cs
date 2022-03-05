using InvoiceLite.Api.Configuration;
using InvoiceLite.Api.Data;
using InvoiceLite.Api.Extensions;
using InvoiceLite.Api.Models;
using InvoiceLite.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InvoiceLite.Api.Controllers;

[ApiController]
[Route("api/invoices/{invoiceId:int}/payment-intent")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPaymentService _payments;
    private readonly StripeOptions _stripe;

    public PaymentsController(AppDbContext db, IPaymentService payments, IOptions<StripeOptions> stripe)
    {
        _db = db;
        _payments = payments;
        _stripe = stripe.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Create(int invoiceId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var invoice = await _db.Invoices
            .Include(i => i.LineItems)
            .SingleOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId, ct);
        if (invoice == null) return NotFound();

        if (invoice.Status != InvoiceStatus.Sent)
        {
            return Problem(
                title: "Invoice is not payable",
                detail: "Only invoices that have been sent can be paid.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var result = await _payments.CreatePaymentIntentAsync(invoice, ct);

        return Ok(new
        {
            result.PaymentIntentId,
            result.ClientSecret,
            result.Amount,
            result.Currency,
            _stripe.PublishableKey
        });
    }
}
