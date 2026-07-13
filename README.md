# mr2008.ir

ASP.NET Core storefront and administration application for Peugeot 2008 parts.

## Configuration

Never commit production credentials. Supply the SQL Server connection string at deployment time:

```text
ConnectionStrings__Default=Server=...;Database=...;User Id=...;Password=...;Encrypt=True
```

The repository previously contained a SQL credential. Rotate that SQL password before deploying this revision.

## Database bootstrap

Bootstrap data is additive-only and disabled by default. For a new empty database, temporarily set:

```text
Database__SeedOnStartup=true
```

Disable it again after the initial bootstrap. Existing products, categories, articles and pages are never deleted by the bootstrapper.

## External product images

In Admin → Media, use **Store external images in database**. The action imports only approved Part Online product-image URLs that do not already have database storage. It is idempotent, limited to 50 images per run and 10 MB per image.

## Commerce flow

Fixed-price products support a session-backed cart, server-validated checkout and production Zarinpal request/verify flow. Checkout re-reads every current price, stores immutable line-price snapshots, redirects only to Zarinpal, and marks an order paid only after server-to-server verification. Configure the merchant identifier as a deployment secret:

```text
Payment__Zarinpal__MerchantId=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

Contact-price products use a direct phone link only. Orders have a private tracking token, status timeline, payment history, and an order-number/mobile lookup protected by rate limiting.

## Production checklist

- Apply EF Core migrations before starting the application.
- Verify cart, Zarinpal request/callback/verification, order tracking and Admin → Orders on the production database.
- Provide the connection string through deployment secrets.
- Provide the Zarinpal merchant identifier through deployment secrets; never put it in `appsettings.json`.
- Rotate the previously exposed SQL password.
- Replace placeholder contact details in Admin → Settings.
- Run `dotnet build Atelier.slnx` and smoke-test authentication, catalog routes, image import, sitemap and robots endpoints.
