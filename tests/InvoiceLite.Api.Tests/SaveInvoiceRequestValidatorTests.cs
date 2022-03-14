using InvoiceLite.Api.Dtos;
using InvoiceLite.Api.Validation;
using Xunit;

namespace InvoiceLite.Api.Tests;

public class SaveInvoiceRequestValidatorTests
{
    private readonly SaveInvoiceRequestValidator _validator = new();

    private static SaveInvoiceRequest Valid() => new(
        1,
        new DateTime(2022, 3, 1),
        new DateTime(2022, 3, 31),
        "usd",
        null,
        new List<LineItemRequest> { new("Consulting", 2, 100m) });

    [Fact]
    public void Valid_request_passes()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Due_date_before_issue_date_fails()
    {
        var result = _validator.Validate(Valid() with { DueDate = new DateTime(2022, 2, 1) });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SaveInvoiceRequest.DueDate));
    }

    [Fact]
    public void Invoice_without_line_items_fails()
    {
        var result = _validator.Validate(Valid() with { LineItems = new List<LineItemRequest>() });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SaveInvoiceRequest.LineItems));
    }

    [Fact]
    public void Unsupported_currency_fails()
    {
        var result = _validator.Validate(Valid() with { Currency = "jpy" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SaveInvoiceRequest.Currency));
    }

    [Fact]
    public void Line_item_with_zero_quantity_fails()
    {
        var request = Valid() with { LineItems = new List<LineItemRequest> { new("Consulting", 0, 100m) } };

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "LineItems[0].Quantity");
    }
}
