using InvoiceLite.Api.Data;
using InvoiceLite.Api.Dtos;
using InvoiceLite.Api.Extensions;
using InvoiceLite.Api.Models;
using InvoiceLite.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceLite.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IInvoiceNumberGenerator _numbers;

    public InvoicesController(AppDbContext db, IInvoiceNumberGenerator numbers)
    {
        _db = db;
        _numbers = numbers;
    }

    [HttpGet]
    public async Task<IEnumerable<InvoiceDto>> GetAll()
    {
        var userId = User.GetUserId();
        var invoices = await _db.Invoices
            .Where(i => i.UserId == userId)
            .Include(i => i.Client)
            .Include(i => i.LineItems)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
        return invoices.Select(i => i.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDto>> Get(int id)
    {
        var invoice = await LoadInvoice(id);
        if (invoice == null) return NotFound();
        return invoice.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create(SaveInvoiceRequest request)
    {
        var userId = User.GetUserId();
        var client = await FindClient(request.ClientId);
        if (client == null) return BadRequest(new { message = "Client not found." });

        var invoice = new Invoice
        {
            UserId = userId,
            ClientId = client.Id,
            Number = await _numbers.NextAsync(userId),
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            Currency = request.Currency?.ToLowerInvariant() ?? "usd",
            Notes = request.Notes,
            LineItems = request.LineItems.Select(ToLineItem).ToList()
        };
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = invoice.Id }, invoice.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InvoiceDto>> Update(int id, SaveInvoiceRequest request)
    {
        var invoice = await LoadInvoice(id);
        if (invoice == null) return NotFound();

        var client = await FindClient(request.ClientId);
        if (client == null) return BadRequest(new { message = "Client not found." });

        invoice.ClientId = client.Id;
        invoice.Client = client;
        invoice.IssueDate = request.IssueDate;
        invoice.DueDate = request.DueDate;
        invoice.Currency = request.Currency?.ToLowerInvariant() ?? invoice.Currency;
        invoice.Notes = request.Notes;
        invoice.LineItems.Clear();
        invoice.LineItems.AddRange(request.LineItems.Select(ToLineItem));
        await _db.SaveChangesAsync();

        return invoice.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await LoadInvoice(id);
        if (invoice == null) return NotFound();

        _db.Invoices.Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/send")]
    public Task<ActionResult<InvoiceDto>> Send(int id) => Transition(id, InvoiceStatus.Sent);

    [HttpPost("{id:int}/mark-paid")]
    public Task<ActionResult<InvoiceDto>> MarkPaid(int id) => Transition(id, InvoiceStatus.Paid);

    [HttpPost("{id:int}/void")]
    public Task<ActionResult<InvoiceDto>> Void(int id) => Transition(id, InvoiceStatus.Void);

    private async Task<ActionResult<InvoiceDto>> Transition(int id, InvoiceStatus target)
    {
        var invoice = await LoadInvoice(id);
        if (invoice == null) return NotFound();

        if (!InvoiceWorkflow.CanTransition(invoice.Status, target))
        {
            return Problem(
                title: "Invalid status transition",
                detail: $"Cannot move invoice from {invoice.Status} to {target}.",
                statusCode: StatusCodes.Status409Conflict);
        }

        invoice.Status = target;
        if (target == InvoiceStatus.Sent) invoice.SentAt = DateTime.UtcNow;
        if (target == InvoiceStatus.Paid) invoice.PaidAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return invoice.ToDto();
    }

    private Task<Invoice?> LoadInvoice(int id)
    {
        var userId = User.GetUserId();
        return _db.Invoices
            .Include(i => i.Client)
            .Include(i => i.LineItems)
            .SingleOrDefaultAsync(i => i.Id == id && i.UserId == userId);
    }

    private Task<Client?> FindClient(int clientId)
    {
        var userId = User.GetUserId();
        return _db.Clients.SingleOrDefaultAsync(c => c.Id == clientId && c.UserId == userId);
    }

    private static InvoiceLineItem ToLineItem(LineItemRequest li) => new()
    {
        Description = li.Description.Trim(),
        Quantity = li.Quantity,
        UnitPrice = li.UnitPrice
    };
}
