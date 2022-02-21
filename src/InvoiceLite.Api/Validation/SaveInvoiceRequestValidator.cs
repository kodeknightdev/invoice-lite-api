using FluentValidation;
using InvoiceLite.Api.Dtos;

namespace InvoiceLite.Api.Validation;

public class SaveInvoiceRequestValidator : AbstractValidator<SaveInvoiceRequest>
{
    private static readonly string[] SupportedCurrencies = { "usd", "eur", "gbp", "cad" };

    public SaveInvoiceRequestValidator()
    {
        RuleFor(x => x.ClientId).GreaterThan(0);
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("Due date cannot be before the issue date.");
        RuleFor(x => x.Currency)
            .Must(c => c == null || SupportedCurrencies.Contains(c.ToLowerInvariant()))
            .WithMessage("Currency must be one of: usd, eur, gbp, cad.");
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.LineItems)
            .NotEmpty()
            .WithMessage("An invoice needs at least one line item.");
        RuleForEach(x => x.LineItems).SetValidator(new LineItemRequestValidator());
    }
}

public class LineItemRequestValidator : AbstractValidator<LineItemRequest>
{
    public LineItemRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
