namespace InvoiceLite.Api.Configuration;

public class JwtOptions
{
    public string Issuer { get; set; } = "invoice-lite";
    public string Audience { get; set; } = "invoice-lite-web";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 120;
}
