using InvoiceLite.Api.Configuration;
using InvoiceLite.Api.Data;
using InvoiceLite.Api.Models;
using Microsoft.Extensions.Options;

namespace InvoiceLite.Api.Services;

public record PaymentIntentResult(string PaymentIntentId, string ClientSecret, long Amount, string Currency);

public interface IPaymentService
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(Invoice invoice, CancellationToken ct = default);
}

public class StripePaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly StripeOptions _options;

    public StripePaymentService(AppDbContext db, IOptions<StripeOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(Invoice invoice, CancellationToken ct = default)
    {
        var amount = (long)invoice.Total;
        var service = new Stripe.PaymentIntentService(new Stripe.StripeClient(_options.SecretKey));

        var intent = await service.CreateAsync(new Stripe.PaymentIntentCreateOptions
        {
            Amount = amount,
            Currency = invoice.Currency,
            Description = $"Invoice {invoice.Number}",
            AutomaticPaymentMethods = new Stripe.PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            Metadata = new Dictionary<string, string>
            {
                ["invoice_id"] = invoice.Id.ToString(),
                ["invoice_number"] = invoice.Number
            }
        }, cancellationToken: ct);

        _db.Payments.Add(new Payment
        {
            InvoiceId = invoice.Id,
            StripePaymentIntentId = intent.Id,
            AmountCents = amount,
            Currency = invoice.Currency,
            Status = intent.Status
        });
        await _db.SaveChangesAsync(ct);

        return new PaymentIntentResult(intent.Id, intent.ClientSecret, amount, invoice.Currency);
    }
}
