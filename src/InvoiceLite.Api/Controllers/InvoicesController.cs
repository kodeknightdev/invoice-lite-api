using InvoiceLite.Api.Data;
using InvoiceLite.Api.Dtos;
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
        var invoices = await _db.Invoices
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
        var client = await _db.Clients.FindAsync(request.ClientId);
        if (client == null) return BadRequest(new { message = "Client not found." });

        var invoice = new Invoice
        {
            ClientId = client.Id,
            Number = await _numbers.NextAsync(),
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            Currency = request.Currency ?? "usd",
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

        invoice.ClientId = request.ClientId;
        invoice.IssueDate = request.IssueDate;
        invoice.DueDate = request.DueDate;
        invoice.Currency = request.Currency ?? invoice.Currency;
        invoice.Notes = request.Notes;
        invoice.LineItems.Clear();
        invoice.LineItems.AddRange(request.LineItems.Select(ToLineItem));
        await _db.SaveChangesAsync();

        return invoice.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();

        _db.Invoices.Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private Task<Invoice?> LoadInvoice(int id) =>
        _db.Invoices
            .Include(i => i.Client)
            .Include(i => i.LineItems)
            .SingleOrDefaultAsync(i => i.Id == id);

    private static InvoiceLineItem ToLineItem(LineItemRequest li) => new()
    {
        Description = li.Description,
        Quantity = li.Quantity,
        UnitPrice = li.UnitPrice
    };
}
