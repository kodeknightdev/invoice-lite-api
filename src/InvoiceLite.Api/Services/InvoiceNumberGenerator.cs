using InvoiceLite.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceLite.Api.Services;

public interface IInvoiceNumberGenerator
{
    Task<string> NextAsync(int userId, CancellationToken ct = default);
}

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private const string Prefix = "INV-";

    private readonly AppDbContext _db;

    public InvoiceNumberGenerator(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> NextAsync(int userId, CancellationToken ct = default)
    {
        // Use the highest existing number rather than a count so that deleting
        // an invoice doesn't cause the next one to reuse a number.
        var numbers = await _db.Invoices
            .Where(i => i.UserId == userId)
            .Select(i => i.Number)
            .ToListAsync(ct);

        var highest = numbers
            .Select(n => int.TryParse(n.Replace(Prefix, string.Empty), out var value) ? value : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{Prefix}{highest + 1:D4}";
    }
}
