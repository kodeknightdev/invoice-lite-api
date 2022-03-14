using InvoiceLite.Api.Models;
using Xunit;

namespace InvoiceLite.Api.Tests;

public class InvoiceTotalsTests
{
    [Fact]
    public void Line_amount_is_quantity_times_unit_price()
    {
        var item = new InvoiceLineItem { Quantity = 3m, UnitPrice = 19.99m };

        Assert.Equal(59.97m, item.Amount);
    }

    [Fact]
    public void Line_amount_rounds_half_cents_away_from_zero()
    {
        var item = new InvoiceLineItem { Quantity = 1.5m, UnitPrice = 33.33m }; // 49.995

        Assert.Equal(50.00m, item.Amount);
    }

    [Fact]
    public void Invoice_total_sums_rounded_line_amounts()
    {
        var invoice = new Invoice
        {
            LineItems =
            {
                new InvoiceLineItem { Quantity = 1.5m, UnitPrice = 33.33m },
                new InvoiceLineItem { Quantity = 0.25m, UnitPrice = 120m },
                new InvoiceLineItem { Quantity = 2m, UnitPrice = 0.005m }
            }
        };

        Assert.Equal(80.01m, invoice.Total);
    }

    [Fact]
    public void Empty_invoice_totals_zero()
    {
        Assert.Equal(0m, new Invoice().Total);
    }
}
