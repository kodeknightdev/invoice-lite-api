using InvoiceLite.Api.Data;
using InvoiceLite.Api.Dtos;
using InvoiceLite.Api.Models;
using InvoiceLite.Api.Services;
using Xunit;

namespace InvoiceLite.Api.Tests;

public class InvoiceServiceTests : IDisposable
{
    private const int UserId = 1;
    private const int OtherUserId = 2;
    private const int ClientId = 10;
    private const int OtherUsersClientId = 20;

    private readonly AppDbContext _db;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _db = TestDb.Create();
        _db.Clients.AddRange(
            new Client { Id = ClientId, UserId = UserId, Name = "Acme Co", Email = "billing@acme.test" },
            new Client { Id = OtherUsersClientId, UserId = OtherUserId, Name = "Globex", Email = "ap@globex.test" });
        _db.SaveChanges();

        _service = new InvoiceService(_db, new InvoiceNumberGenerator(_db));
    }

    public void Dispose() => _db.Dispose();

    private static SaveInvoiceRequest NewRequest(int clientId = ClientId) => new(
        clientId,
        new DateTime(2022, 3, 1),
        new DateTime(2022, 3, 31),
        "USD",
        null,
        new List<LineItemRequest> { new("Website redesign", 10, 85m) });

    [Fact]
    public async Task CreateAsync_creates_a_numbered_draft()
    {
        var invoice = await _service.CreateAsync(UserId, NewRequest());

        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Equal("INV-0001", invoice.Number);
        Assert.Equal("usd", invoice.Currency);
        Assert.Equal("Acme Co", invoice.Client?.Name);
        Assert.Equal(850m, invoice.Total);
    }

    [Fact]
    public async Task CreateAsync_rejects_a_client_owned_by_someone_else()
    {
        await Assert.ThrowsAsync<InvoiceRuleException>(
            () => _service.CreateAsync(UserId, NewRequest(OtherUsersClientId)));
    }

    [Fact]
    public async Task Send_then_mark_paid_records_timestamps()
    {
        var invoice = await _service.CreateAsync(UserId, NewRequest());

        await _service.TransitionAsync(UserId, invoice.Id, InvoiceStatus.Sent);
        var paid = await _service.TransitionAsync(UserId, invoice.Id, InvoiceStatus.Paid);

        Assert.NotNull(paid);
        Assert.Equal(InvoiceStatus.Paid, paid!.Status);
        Assert.NotNull(paid.SentAt);
        Assert.NotNull(paid.PaidAt);
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft, InvoiceStatus.Paid)]
    [InlineData(InvoiceStatus.Paid, InvoiceStatus.Void)]
    [InlineData(InvoiceStatus.Void, InvoiceStatus.Sent)]
    [InlineData(InvoiceStatus.Sent, InvoiceStatus.Draft)]
    public async Task Invalid_transitions_are_rejected(InvoiceStatus from, InvoiceStatus to)
    {
        var invoice = await _service.CreateAsync(UserId, NewRequest());
        invoice.Status = from;
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvoiceRuleException>(
            () => _service.TransitionAsync(UserId, invoice.Id, to));
    }

    [Fact]
    public async Task Users_cannot_touch_each_others_invoices()
    {
        var invoice = await _service.CreateAsync(UserId, NewRequest());

        Assert.Null(await _service.GetAsync(OtherUserId, invoice.Id));
        Assert.Null(await _service.TransitionAsync(OtherUserId, invoice.Id, InvoiceStatus.Sent));
        Assert.False(await _service.DeleteAsync(OtherUserId, invoice.Id));
    }

    [Fact]
    public async Task Sent_invoices_cannot_be_edited_or_deleted()
    {
        var invoice = await _service.CreateAsync(UserId, NewRequest());
        await _service.TransitionAsync(UserId, invoice.Id, InvoiceStatus.Sent);

        await Assert.ThrowsAsync<InvoiceRuleException>(
            () => _service.UpdateAsync(UserId, invoice.Id, NewRequest()));
        await Assert.ThrowsAsync<InvoiceRuleException>(
            () => _service.DeleteAsync(UserId, invoice.Id));
    }

    [Fact]
    public async Task ListAsync_returns_requested_page_and_total()
    {
        for (var i = 0; i < 5; i++)
            await _service.CreateAsync(UserId, NewRequest());

        var result = await _service.ListAsync(UserId, new InvoiceQuery { Page = 2, PageSize = 2 });

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(new[] { "INV-0003", "INV-0002" }, result.Items.Select(i => i.Number));
    }
}
