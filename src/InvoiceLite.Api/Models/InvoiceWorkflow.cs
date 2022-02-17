namespace InvoiceLite.Api.Models;

public static class InvoiceWorkflow
{
    private static readonly Dictionary<InvoiceStatus, InvoiceStatus[]> Allowed = new()
    {
        [InvoiceStatus.Draft] = new[] { InvoiceStatus.Sent, InvoiceStatus.Void },
        [InvoiceStatus.Sent] = new[] { InvoiceStatus.Paid, InvoiceStatus.Void },
        [InvoiceStatus.Paid] = Array.Empty<InvoiceStatus>(),
        [InvoiceStatus.Void] = Array.Empty<InvoiceStatus>()
    };

    public static bool CanTransition(InvoiceStatus from, InvoiceStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}
