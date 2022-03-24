using InvoiceLite.Api.Data;
using InvoiceLite.Api.Dtos;
using InvoiceLite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceLite.Api.Services;

public interface IInvoiceService
{
    Task<List<Invoice>> ListAsync(int userId, CancellationToken ct = default);
    Task<Invoice?> GetAsync(int userId, int id, CancellationToken ct = default);
    Task<Invoice> CreateAsync(int userId, SaveInvoiceRequest request, CancellationToken ct = default);
    Task<Invoice?> UpdateAsync(int userId, int id, SaveInvoiceRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int userId, int id, CancellationToken ct = default);
    Task<Invoice?> TransitionAsync(int userId, int id, InvoiceStatus target, CancellationToken ct = default);
}

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;
    private readonly IInvoiceNumberGenerator _numbers;

    public InvoiceService(AppDbContext db, IInvoiceNumberGenerator numbers)
    {
        _db = db;
        _numbers = numbers;
    }

    public Task<List<Invoice>> ListAsync(int userId, CancellationToken ct = default) =>
        _db.Invoices
            .Where(i => i.UserId == userId)
            .Include(i => i.Client)
            .Include(i => i.LineItems)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(ct);

    public Task<Invoice?> GetAsync(int userId, int id, CancellationToken ct = default) =>
        _db.Invoices
            .Include(i => i.Client)
            .Include(i => i.LineItems)
            .SingleOrDefaultAsync(i => i.Id == id && i.UserId == userId, ct);

    public async Task<Invoice> CreateAsync(int userId, SaveInvoiceRequest request, CancellationToken ct = default)
    {
        await EnsureClientBelongsToUser(userId, request.ClientId, ct);

        var invoice = new Invoice
        {
            UserId = userId,
            ClientId = request.ClientId,
            Number = await _numbers.NextAsync(userId, ct),
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            Currency = request.Currency?.ToLowerInvariant() ?? "usd",
            Notes = request.Notes,
            LineItems = request.LineItems.Select(ToLineItem).ToList()
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(invoice).Reference(i => i.Client).LoadAsync(ct);
        return invoice;
    }

    public async Task<Invoice?> UpdateAsync(int userId, int id, SaveInvoiceRequest request, CancellationToken ct = default)
    {
        var invoice = await GetAsync(userId, id, ct);
        if (invoice == null) return null;

        EnsureDraft(invoice, "edited");
        await EnsureClientBelongsToUser(userId, request.ClientId, ct);

        invoice.ClientId = request.ClientId;
        invoice.IssueDate = request.IssueDate;
        invoice.DueDate = request.DueDate;
        invoice.Currency = request.Currency?.ToLowerInvariant() ?? invoice.Currency;
        invoice.Notes = request.Notes;
        invoice.LineItems.Clear();
        invoice.LineItems.AddRange(request.LineItems.Select(ToLineItem));

        await _db.SaveChangesAsync(ct);
        await _db.Entry(invoice).Reference(i => i.Client).LoadAsync(ct);
        return invoice;
    }

    public async Task<bool> DeleteAsync(int userId, int id, CancellationToken ct = default)
    {
        var invoice = await GetAsync(userId, id, ct);
        if (invoice == null) return false;

        EnsureDraft(invoice, "deleted");
        _db.Invoices.Remove(invoice);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Invoice?> TransitionAsync(int userId, int id, InvoiceStatus target, CancellationToken ct = default)
    {
        var invoice = await GetAsync(userId, id, ct);
        if (invoice == null) return null;

        if (!InvoiceWorkflow.CanTransition(invoice.Status, target))
        {
            throw new InvoiceRuleException(
                "Invalid status transition",
                $"Cannot move invoice from {invoice.Status} to {target}.");
        }

        invoice.Status = target;
        if (target == InvoiceStatus.Sent) invoice.SentAt = DateTime.UtcNow;
        if (target == InvoiceStatus.Paid) invoice.PaidAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return invoice;
    }

    private async Task EnsureClientBelongsToUser(int userId, int clientId, CancellationToken ct)
    {
        if (!await _db.Clients.AnyAsync(c => c.Id == clientId && c.UserId == userId, ct))
        {
            throw new InvoiceRuleException(
                "Client not found",
                "The selected client does not exist.",
                StatusCodes.Status400BadRequest);
        }
    }

    private static void EnsureDraft(Invoice invoice, string action)
    {
        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvoiceRuleException(
                "Invoice is locked",
                $"Only draft invoices can be {action}. This one is {invoice.Status}.");
        }
    }

    private static InvoiceLineItem ToLineItem(LineItemRequest li) => new()
    {
        Description = li.Description.Trim(),
        Quantity = li.Quantity,
        UnitPrice = li.UnitPrice
    };
}
