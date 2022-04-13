using InvoiceLite.Api.Dtos;
using InvoiceLite.Api.Extensions;
using InvoiceLite.Api.Models;
using InvoiceLite.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceLite.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoices;

    public InvoicesController(IInvoiceService invoices)
    {
        _invoices = invoices;
    }

    [HttpGet]
    public async Task<PagedResult<InvoiceDto>> GetAll([FromQuery] InvoiceQuery query, CancellationToken ct)
    {
        var result = await _invoices.ListAsync(User.GetUserId(), query, ct);
        return new PagedResult<InvoiceDto>(
            result.Items.Select(i => i.ToDto()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDto>> Get(int id, CancellationToken ct)
    {
        var invoice = await _invoices.GetAsync(User.GetUserId(), id, ct);
        if (invoice == null) return NotFound();
        return invoice.ToDto();
    }

    [HttpPost]
    public Task<ActionResult<InvoiceDto>> Create(SaveInvoiceRequest request, CancellationToken ct) =>
        Execute(async () =>
        {
            var invoice = await _invoices.CreateAsync(User.GetUserId(), request, ct);
            return CreatedAtAction(nameof(Get), new { id = invoice.Id }, invoice.ToDto());
        });

    [HttpPut("{id:int}")]
    public Task<ActionResult<InvoiceDto>> Update(int id, SaveInvoiceRequest request, CancellationToken ct) =>
        Execute(async () =>
        {
            var invoice = await _invoices.UpdateAsync(User.GetUserId(), id, request, ct);
            if (invoice == null) return NotFound();
            return invoice.ToDto();
        });

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            return await _invoices.DeleteAsync(User.GetUserId(), id, ct) ? NoContent() : NotFound();
        }
        catch (InvoiceRuleException ex)
        {
            return Problem(title: ex.Title, detail: ex.Message, statusCode: ex.StatusCode);
        }
    }

    [HttpPost("{id:int}/send")]
    public Task<ActionResult<InvoiceDto>> Send(int id, CancellationToken ct) =>
        Transition(id, InvoiceStatus.Sent, ct);

    [HttpPost("{id:int}/mark-paid")]
    public Task<ActionResult<InvoiceDto>> MarkPaid(int id, CancellationToken ct) =>
        Transition(id, InvoiceStatus.Paid, ct);

    [HttpPost("{id:int}/void")]
    public Task<ActionResult<InvoiceDto>> Void(int id, CancellationToken ct) =>
        Transition(id, InvoiceStatus.Void, ct);

    private Task<ActionResult<InvoiceDto>> Transition(int id, InvoiceStatus target, CancellationToken ct) =>
        Execute(async () =>
        {
            var invoice = await _invoices.TransitionAsync(User.GetUserId(), id, target, ct);
            if (invoice == null) return NotFound();
            return invoice.ToDto();
        });

    private async Task<ActionResult<InvoiceDto>> Execute(Func<Task<ActionResult<InvoiceDto>>> action)
    {
        try
        {
            return await action();
        }
        catch (InvoiceRuleException ex)
        {
            return Problem(title: ex.Title, detail: ex.Message, statusCode: ex.StatusCode);
        }
    }
}
