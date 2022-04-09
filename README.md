# invoice-lite-api

Backend for Invoice Lite, a small invoicing tool for freelancers. You can manage clients, build invoices, move them through a draft → sent → paid workflow and take card payments with Stripe (test mode only).

The React frontend is a separate repo, `invoice-lite-web`.

## Stack

- ASP.NET Core 6 Web API (.NET 6 LTS)
- Entity Framework Core 6 + SQL Server (LocalDB by default)
- Stripe.net: PaymentIntents + webhooks, **test mode only**
- JWT bearer auth, BCrypt password hashing
- FluentValidation
- xUnit + EF Core InMemory for tests

## Getting started

Requirements: .NET 6 SDK and SQL Server or LocalDB.

```bash
dotnet tool restore
dotnet ef database update -p src/InvoiceLite.Api
dotnet run --project src/InvoiceLite.Api
```

Swagger UI is at https://localhost:7180/swagger. Register via `/api/auth/register`, then use **Authorize** with the returned token.

If you're running SQL Server in Docker instead of LocalDB, override the connection string with user-secrets:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=InvoiceLite;User Id=sa;Password=<your-password>;TrustServerCertificate=True" --project src/InvoiceLite.Api
```

## Stripe (test mode)

Only use **test** keys (`sk_test_...` / `pk_test_...`). The values in `appsettings.json` are placeholders. Put your real test keys in user-secrets:

```bash
cd src/InvoiceLite.Api
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_..."
```

To receive webhooks locally, run the Stripe CLI and copy the signing secret it prints:

```bash
stripe listen --forward-to https://localhost:7180/api/webhooks/stripe
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..."
```

Test card: `4242 4242 4242 4242`, any future expiry, any CVC.

## API overview

| Method | Route | Notes |
| --- | --- | --- |
| POST | `/api/auth/register`, `/api/auth/login` | returns a JWT |
| GET, POST, PUT, DELETE | `/api/clients` | |
| GET | `/api/invoices?page=&pageSize=&status=&clientId=` | paged |
| GET, POST, PUT, DELETE | `/api/invoices/{id}` | only drafts can be edited or deleted |
| POST | `/api/invoices/{id}/send`, `/mark-paid`, `/void` | status workflow |
| POST | `/api/invoices/{id}/payment-intent` | Stripe PaymentIntent for a sent invoice |
| POST | `/api/webhooks/stripe` | `payment_intent.succeeded` marks the invoice paid |
| GET | `/api/dashboard` | totals for the signed-in user |

Invoice workflow: `Draft → Sent → Paid`. Draft and Sent invoices can also be voided.

## Tests

```bash
dotnet test
```
