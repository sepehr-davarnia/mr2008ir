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

## Production checklist

- Apply EF Core migrations before starting the application.
- Provide the connection string through deployment secrets.
- Rotate the previously exposed SQL password.
- Replace placeholder contact details in Admin → Settings.
- Run `dotnet build Atelier.slnx` and smoke-test authentication, catalog routes, image import, sitemap and robots endpoints.
