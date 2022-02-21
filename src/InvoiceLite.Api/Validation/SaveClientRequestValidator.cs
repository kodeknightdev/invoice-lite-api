using FluentValidation;
using InvoiceLite.Api.Dtos;

namespace InvoiceLite.Api.Validation;

public class SaveClientRequestValidator : AbstractValidator<SaveClientRequest>
{
    public SaveClientRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Company).MaximumLength(200);
        RuleFor(x => x.Address).MaximumLength(500);
    }
}
