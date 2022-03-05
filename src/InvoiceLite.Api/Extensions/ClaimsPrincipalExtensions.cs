using System.Security.Claims;

namespace InvoiceLite.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("Request does not carry a user id claim.");
    }
}
