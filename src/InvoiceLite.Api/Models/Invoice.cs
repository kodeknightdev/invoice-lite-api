using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceLite.Api.Models;

public class Invoice
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Number { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Currency { get; set; } = "usd";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public List<InvoiceLineItem> LineItems { get; set; } = new();

    [NotMapped]
    public decimal Total => LineItems.Sum(li => li.Amount);
}
