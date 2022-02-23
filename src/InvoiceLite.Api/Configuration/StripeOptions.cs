namespace InvoiceLite.Api.Configuration;

/// <summary>
/// Stripe settings. Only test-mode keys (sk_test_/pk_test_) should ever be used
/// with this project; real values belong in user-secrets, not appsettings.
/// </summary>
public class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
