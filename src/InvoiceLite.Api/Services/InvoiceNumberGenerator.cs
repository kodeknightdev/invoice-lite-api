using InvoiceLite.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceLite.Api.Services;

public interface IInvoiceNumberGenerator
{
    Task<string> NextAsync(int userId, CancellationToken ct = default);
}

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly AppDbContext _db;

    public InvoiceNumberGenerator(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> NextAsync(int userId, CancellationToken ct = default)
    {
        var count = await _db.Invoices.CountAsync(i => i.UserId == userId, ct);
        return $"INV-{count + 1:D4}";
    }
}
