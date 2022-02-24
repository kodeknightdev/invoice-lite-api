using InvoiceLite.Api.Configuration;
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
    private readonly StripeOptions _options;

    public StripePaymentService(IOptions<StripeOptions> options)
    {
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

        return new PaymentIntentResult(intent.Id, intent.ClientSecret, amount, invoice.Currency);
    }
}
