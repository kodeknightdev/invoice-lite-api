using InvoiceLite.Api.Models;
using Xunit;

namespace InvoiceLite.Api.Tests;

public class InvoiceOverdueTests
{
    [Fact]
    public void Sent_invoice_due_today_is_not_overdue()
    {
        var invoice = new Invoice { Status = InvoiceStatus.Sent, DueDate = DateTime.UtcNow.Date };

        Assert.False(invoice.IsOverdue);
    }

    [Fact]
    public void Sent_invoice_due_yesterday_is_overdue()
    {
        var invoice = new Invoice { Status = InvoiceStatus.Sent, DueDate = DateTime.UtcNow.Date.AddDays(-1) };

        Assert.True(invoice.IsOverdue);
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Paid)]
    [InlineData(InvoiceStatus.Void)]
    public void Only_sent_invoices_can_be_overdue(InvoiceStatus status)
    {
        var invoice = new Invoice { Status = status, DueDate = DateTime.UtcNow.Date.AddDays(-30) };

        Assert.False(invoice.IsOverdue);
    }
}
