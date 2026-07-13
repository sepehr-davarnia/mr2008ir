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

Fixed-price products support a session-backed cart and server-validated checkout. Checkout re-reads every current product price before creating an order, stores immutable line-price snapshots, and exposes fulfillment status in Admin → Orders. Contact-price products and uncertain buyers can call directly, open WhatsApp, or submit a callback request.

The confirmation page deliberately does not claim that payment has completed. Stock and vehicle compatibility are verified first, then staff send the customer the official secure payment link. Connect a licensed Iranian payment gateway before changing this workflow to immediate card payment.

## Production checklist

- Apply EF Core migrations before starting the application.
- Verify cart, checkout, callback requests and Admin → Orders on the production database.
- Provide the connection string through deployment secrets.
- Rotate the previously exposed SQL password.
- Replace placeholder contact details in Admin → Settings.
- Run `dotnet build Atelier.slnx` and smoke-test authentication, catalog routes, image import, sitemap and robots endpoints.
