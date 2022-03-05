namespace InvoiceLite.Api.Models;

public class Client
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Invoice> Invoices { get; set; } = new();
}
