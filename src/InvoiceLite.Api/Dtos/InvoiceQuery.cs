using InvoiceLite.Api.Models;

namespace InvoiceLite.Api.Dtos;

public class InvoiceQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public InvoiceStatus? Status { get; set; }
    public int? ClientId { get; set; }
}
