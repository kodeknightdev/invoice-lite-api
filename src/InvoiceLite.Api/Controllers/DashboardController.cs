using InvoiceLite.Api.Data;
using InvoiceLite.Api.Dtos;
using InvoiceLite.Api.Extensions;
using InvoiceLite.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceLite.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> Get(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var invoices = await _db.Invoices
            .Where(i => i.UserId == userId && i.Status != InvoiceStatus.Void)
            .Include(i => i.LineItems)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        return new DashboardSummaryDto(
            OutstandingTotal: invoices.Where(i => i.Status == InvoiceStatus.Sent).Sum(i => i.Total),
            PaidThisMonth: invoices.Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt >= monthStart).Sum(i => i.Total),
            OverdueCount: invoices.Count(i => i.IsOverdue),
            DraftCount: invoices.Count(i => i.Status == InvoiceStatus.Draft),
            ClientCount: await _db.Clients.CountAsync(c => c.UserId == userId, ct));
    }
}
