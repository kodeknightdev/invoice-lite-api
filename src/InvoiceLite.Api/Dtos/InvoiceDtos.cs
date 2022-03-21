using InvoiceLite.Api.Models;

namespace InvoiceLite.Api.Dtos;

public record LineItemDto(int Id, string Description, decimal Quantity, decimal UnitPrice, decimal Amount);

public record InvoiceDto(
    int Id,
    string Number,
    int ClientId,
    string? ClientName,
    InvoiceStatus Status,
    bool IsOverdue,
    DateTime IssueDate,
    DateTime DueDate,
    string Currency,
    string? Notes,
    decimal Total,
    DateTime? SentAt,
    DateTime? PaidAt,
    IReadOnlyList<LineItemDto> LineItems);

public record LineItemRequest(string Description, decimal Quantity, decimal UnitPrice);

public record SaveInvoiceRequest(
    int ClientId,
    DateTime IssueDate,
    DateTime DueDate,
    string? Currency,
    string? Notes,
    List<LineItemRequest> LineItems);

public static class InvoiceMappings
{
    public static InvoiceDto ToDto(this Invoice i) => new(
        i.Id,
        i.Number,
        i.ClientId,
        i.Client?.Name,
        i.Status,
        i.IsOverdue,
        i.IssueDate,
        i.DueDate,
        i.Currency,
        i.Notes,
        i.Total,
        i.SentAt,
        i.PaidAt,
        i.LineItems
            .Select(li => new LineItemDto(li.Id, li.Description, li.Quantity, li.UnitPrice, li.Amount))
            .ToList());
}
