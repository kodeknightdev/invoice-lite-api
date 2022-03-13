using InvoiceLite.Api.Models;
using InvoiceLite.Api.Services;
using Xunit;

namespace InvoiceLite.Api.Tests;

public class InvoiceNumberGeneratorTests
{
    [Fact]
    public async Task First_invoice_for_a_user_is_INV_0001()
    {
        await using var db = TestDb.Create();

        Assert.Equal("INV-0001", await new InvoiceNumberGenerator(db).NextAsync(userId: 1));
    }

    [Fact]
    public async Task Next_number_follows_the_highest_existing_number()
    {
        await using var db = TestDb.Create();
        db.Invoices.AddRange(
            new Invoice { UserId = 1, Number = "INV-0001" },
            new Invoice { UserId = 1, Number = "INV-0003" });
        await db.SaveChangesAsync();

        Assert.Equal("INV-0004", await new InvoiceNumberGenerator(db).NextAsync(userId: 1));
    }

    [Fact]
    public async Task Numbers_are_scoped_per_user()
    {
        await using var db = TestDb.Create();
        db.Invoices.Add(new Invoice { UserId = 1, Number = "INV-0007" });
        await db.SaveChangesAsync();

        Assert.Equal("INV-0001", await new InvoiceNumberGenerator(db).NextAsync(userId: 2));
    }
}
