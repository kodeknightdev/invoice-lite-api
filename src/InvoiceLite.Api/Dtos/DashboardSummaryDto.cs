namespace InvoiceLite.Api.Dtos;

public record DashboardSummaryDto(
    decimal OutstandingTotal,
    decimal PaidThisMonth,
    int OverdueCount,
    int DraftCount,
    int ClientCount);
