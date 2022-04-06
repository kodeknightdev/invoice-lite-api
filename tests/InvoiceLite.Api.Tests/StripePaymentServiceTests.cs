using System.Globalization;
using InvoiceLite.Api.Configuration;
using InvoiceLite.Api.Models;
using InvoiceLite.Api.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace InvoiceLite.Api.Tests;

public class StripePaymentServiceTests
{
    [Theory]
    [InlineData("19.99", 1999)]
    [InlineData("12.345", 1235)]
    [InlineData("250", 25000)]
    public void ToMinorUnits_converts_to_cents(string amount, long expected)
    {
        var value = decimal.Parse(amount, CultureInfo.InvariantCulture);

        Assert.Equal(expected, StripePaymentService.ToMinorUnits(value));
    }

    [Fact]
    public async Task MarkPaymentSucceededAsync_marks_sent_invoice_paid()
    {
        await using var db = TestDb.Create();
        var invoice = await SeedPayment(db, InvoiceStatus.Sent, "pi_test_123");
        var service = new StripePaymentService(db, Options.Create(new StripeOptions()));

        Assert.True(await service.MarkPaymentSucceededAsync("pi_test_123"));
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.NotNull(invoice.PaidAt);
        Assert.Equal("succeeded", db.Payments.Single().Status);
    }

    [Fact]
    public async Task MarkPaymentSucceededAsync_leaves_void_invoice_alone()
    {
        await using var db = TestDb.Create();
        var invoice = await SeedPayment(db, InvoiceStatus.Void, "pi_test_456");
        var service = new StripePaymentService(db, Options.Create(new StripeOptions()));

        await service.MarkPaymentSucceededAsync("pi_test_456");

        Assert.Equal(InvoiceStatus.Void, invoice.Status);
        Assert.Null(invoice.PaidAt);
    }

    [Fact]
    public async Task MarkPaymentSucceededAsync_returns_false_for_unknown_intent()
    {
        await using var db = TestDb.Create();
        var service = new StripePaymentService(db, Options.Create(new StripeOptions()));

        Assert.False(await service.MarkPaymentSucceededAsync("pi_test_missing"));
    }

    private static async Task<Invoice> SeedPayment(Data.AppDbContext db, InvoiceStatus status, string intentId)
    {
        var invoice = new Invoice { UserId = 1, ClientId = 1, Number = "INV-0001", Status = status };
        db.Invoices.Add(invoice);
        db.Payments.Add(new Payment { Invoice = invoice, StripePaymentIntentId = intentId, AmountCents = 5000 });
        await db.SaveChangesAsync();
        return invoice;
    }
}
