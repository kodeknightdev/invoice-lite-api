namespace InvoiceLite.Api.Models;

public class Payment
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public long AmountCents { get; set; }
    public string Currency { get; set; } = "usd";
    public string Status { get; set; } = "requires_payment_method";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
