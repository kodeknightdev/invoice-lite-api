namespace InvoiceLite.Api.Services;

/// <summary>
/// Thrown when a request is well-formed but breaks an invoice business rule
/// (bad status transition, client not owned by the user, ...).
/// </summary>
public class InvoiceRuleException : Exception
{
    public InvoiceRuleException(string title, string message, int statusCode = StatusCodes.Status409Conflict)
        : base(message)
    {
        Title = title;
        StatusCode = statusCode;
    }

    public string Title { get; }
    public int StatusCode { get; }
}
