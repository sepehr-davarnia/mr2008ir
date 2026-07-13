using Microsoft.EntityFrameworkCore;

namespace Atelier.Infrastructure.Data.Seeding;

/// <summary>Additive bootstrap data for new mr2008.ir environments. Never removes existing business data.</summary>
public static class AtelierDbSeeder
{
    public static async Task SeedAsync(AtelierDbContext dbContext)
    {
        await SeedSettingsAsync(dbContext);
        var categories = await SeedCategoriesAsync(dbContext);
        var vehicles = await SeedVehiclesAsync(dbContext);
        var media = await SeedProductMediaAsync(dbContext);
        await SeedProductsAsync(dbContext, media, categories, vehicles);
        await SeedPagesAsync(dbContext);
        await SeedArticlesAsync(dbContext);
    }

    private static async Task SeedSettingsAsync(AtelierDbContext dbContext)
    {
        var settings = await dbContext.SiteSettings.OrderBy(item => item.Id).FirstOrDefaultAsync();
        if (settings is null)
        {
            settings = new SiteSetting(5120);
            settings.UpdateContactInfo("mr2008.ir", "تهران", null, null, null,
                "https://instagram.com/mr2008.ir", null, "info@mr2008.ir");
            dbContext.SiteSettings.Add(settings);
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(AtelierDbContext dbContext)
    {
        var seeds = new[]
        {
            (Name: "قطعات موتور", Slug: "engine-parts"),
            (Name: "ترمز و تعلیق", Slug: "brake-suspension"),
            (Name: "فیلتر و لوازم مصرفی", Slug: "filters-consumables"),
            (Name: "قطعات برقی", Slug: "electrical-parts"),
            (Name: "بدنه و چراغ", Slug: "body-lighting"),
            (Name: "کابین و اکسسوری", Slug: "interior-accessories")
        };
        var existing = await dbContext.Categories.Where(item => seeds.Select(seed => seed.Slug).Contains(item.Slug))
            .ToDictionaryAsync(item => item.Slug, StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            if (existing.TryGetValue(seed.Slug, out var category)) category.UpdateName(seed.Name);
            else dbContext.Categories.Add(new Category(seed.Name, seed.Slug));
        }
        await dbContext.SaveChangesAsync();
        var slugs = seeds.Select(item => item.Slug).ToArray();
        return await dbContext.Categories.Where(item => slugs.Contains(item.Slug)).ToDictionaryAsync(item => item.Slug);
    }

    private static async Task<Dictionary<string, Vehicle>> SeedVehiclesAsync(AtelierDbContext dbContext)
    {
        var seeds = new[]
        {
            new VehicleSeed("پژو", "۲۰۰۸", 2017, 2020, "EP6 1.6 Turbo", "Allure", "peugeot-2008-ep6-allure"),
            new VehicleSeed("پژو", "۲۰۰۸", 2017, 2020, "EP6 1.6 Turbo", "GT Line", "peugeot-2008-ep6-gt-line")
        };
        var slugs = seeds.Select(item => item.Slug).ToArray();
        var existing = await dbContext.Vehicles.Where(item => slugs.Contains(item.Slug)).ToDictionaryAsync(item => item.Slug);
        foreach (var seed in seeds)
        {
            if (existing.TryGetValue(seed.Slug, out var vehicle))
            {
                vehicle.Update(seed.Make, seed.Model, seed.YearFrom, seed.YearTo, seed.Engine, seed.Trim);
                vehicle.SetActive(true);
            }
            else dbContext.Vehicles.Add(new Vehicle(seed.Make, seed.Model, seed.YearFrom, seed.YearTo, seed.Engine, seed.Trim, seed.Slug));
        }
        await dbContext.SaveChangesAsync();
        return await dbContext.Vehicles.Where(item => slugs.Contains(item.Slug)).ToDictionaryAsync(item => item.Slug);
    }

    private static async Task<Dictionary<string, Media>> SeedProductMediaAsync(AtelierDbContext dbContext)
    {
        var seeds = new[]
        {
            new MediaSeed("peugeot-2008-oil-filter", "https://www.partonlineco.com/wp-content/uploads/2022/12/OIL-FILTEr-for-peugeot-2008-1-1-600x600.jpg", "فیلتر روغن پژو ۲۰۰۸ اصلی", "peugeot-2008-oil-filter.jpg"),
            new MediaSeed("peugeot-2008-front-brake-pad", "https://www.partonlineco.com/wp-content/uploads/2022/08/brake-pad-kit-front-for-peugeot508-5-600x600.webp", "لنت ترمز جلو پژو ۲۰۰۸ تکستار", "peugeot-2008-front-brake-pad.webp"),
            new MediaSeed("peugeot-2008-cabin-filter", "https://www.partonlineco.com/wp-content/uploads/2023/01/BATCH-OF-CARBON-FILTERS-for-peugeot-2008-600x600.jpg", "فیلتر کابین کربن فعال پژو ۲۰۰۸", "peugeot-2008-cabin-filter.jpg"),
            new MediaSeed("peugeot-2008-spark-plug", "https://www.partonlineco.com/wp-content/uploads/2023/08/spark-plug-for-peugeot-2008-508-c3-ds-1-600x600.jpg", "شمع موتور پژو ۲۰۰۸ و ۵۰۸", "peugeot-2008-spark-plug.jpg")
        };
        var slugs = seeds.Select(seed => seed.Slug).ToArray();
        var existing = await dbContext.Media.Where(item => slugs.Contains(item.Slug))
            .ToDictionaryAsync(item => item.Slug, StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            if (existing.TryGetValue(seed.Slug, out var item))
            {
                item.UpdateMetadata(seed.Title, seed.Title);
                continue;
            }
            var contentType = seed.FileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ? "image/webp" : "image/jpeg";
            dbContext.Media.Add(new Media(seed.Slug, seed.Url, null, contentType, null, seed.Title, seed.Title,
                seed.FileName, "محصول", seed.Url, true));
        }
        await dbContext.SaveChangesAsync();
        return await dbContext.Media.Where(item => slugs.Contains(item.Slug)).ToDictionaryAsync(item => item.Slug);
    }

    private static async Task SeedProductsAsync(AtelierDbContext dbContext, IReadOnlyDictionary<string, Media> media,
        IReadOnlyDictionary<string, Category> categories, IReadOnlyDictionary<string, Vehicle> vehicles)
    {
        var seeds = new[]
        {
            new ProductSeed("فیلتر روغن پژو ۲۰۰۸ اصلی", "peugeot-2008-genuine-oil-filter", "فیلتر روغن مناسب موتور EP6 پژو ۲۰۰۸ با ضمانت اصالت. مشخصات فنی: خودرو: پژو ۲۰۰۸، موتور: EP6، نوع: فیلتر روغن", 1590000, "peugeot-2008-oil-filter", "پژو", ["engine-parts", "filters-consumables"], false),
            new ProductSeed("لنت ترمز جلو پژو ۲۰۰۸ تکستار", "peugeot-2008-textar-front-brake-pad", "دست کامل لنت ترمز جلو مناسب پژو ۲۰۰۸ با بررسی کد فنی. مشخصات فنی: محور: جلو، برند: تکستار، خودرو: پژو ۲۰۰۸", 5990000, "peugeot-2008-front-brake-pad", "Textar", ["brake-suspension"], true),
            new ProductSeed("فیلتر کابین پژو ۲۰۰۸ کربن فعال", "peugeot-2008-active-carbon-cabin-filter", "فیلتر کابین کربن فعال برای جذب بو و ذرات معلق. مشخصات فنی: خودرو: پژو ۲۰۰۸، نوع: کربن فعال", 1950000, "peugeot-2008-cabin-filter", null, ["filters-consumables"], false),
            new ProductSeed("شمع موتور پژو ۲۰۰۸ و ۵۰۸", "peugeot-2008-508-spark-plug-set", "ست شمع مناسب موتور EP6 با کنترل کد فنی پیش از ارسال. مشخصات فنی: موتور: EP6، خودرو: پژو ۲۰۰۸ و ۵۰۸", 7250000, "peugeot-2008-spark-plug", null, ["engine-parts", "electrical-parts"], true)
        };
        var slugs = seeds.Select(seed => seed.Slug).ToArray();
        var existing = await dbContext.Products.Include(item => item.Gallery).Include(item => item.Categories)
            .Include(item => item.Compatibilities).Where(item => slugs.Contains(item.Slug))
            .ToDictionaryAsync(item => item.Slug, StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            if (!existing.TryGetValue(seed.Slug, out var product))
            {
                product = new Product(seed.Name, seed.Slug, seed.Description);
                dbContext.Products.Add(product);
            }
            else product.UpdateDetails(seed.Name, seed.Description);
            product.UpdateCommerceDetails(seed.Brand, seed.Brand, null, null, null);
            product.SetCategories(seed.CategorySlugs.Where(categories.ContainsKey).Select(slug => categories[slug]));
            product.SetCompatibilities(vehicles.Values.Select(vehicle => new ProductCompatibility(vehicle.Id, seed.RequiresVinCheck,
                seed.RequiresVinCheck ? "تطبیق با VIN پیش از ارسال توصیه می‌شود." : null)));
            product.SetPrice(seed.Price);
            product.Publish();
            var productMedia = media[seed.MediaSlug];
            if (product.Gallery.All(item => item.Id != productMedia.Id)) product.AddMedia(productMedia);
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPagesAsync(AtelierDbContext dbContext)
    {
        var seeds = new[]
        {
            new PageSeed("درباره ما", "about-us", "<p>mr2008.ir یک فروشگاه تخصصی قطعات پژو ۲۰۰۸ است. تمرکز ما بر انتخاب دقیق قطعه، اصالت کالا و مشاوره فنی پیش از خرید است.</p>", "درباره mr2008.ir | فروشگاه تخصصی قطعات پژو ۲۰۰۸", "آشنایی با رویکرد تخصصی mr2008.ir در تأمین و فروش قطعات پژو ۲۰۰۸."),
            new PageSeed("تماس با ما", "contact-us", "<p>برای استعلام موجودی، بررسی کد فنی و دریافت مشاوره خرید با کارشناسان ما در ارتباط باشید.</p>", "تماس با mr2008.ir", "راه‌های تماس و دریافت مشاوره تخصصی خرید قطعات پژو ۲۰۰۸."),
            new PageSeed("سؤالات متداول", "faq", "<h2>چطور از سازگاری قطعه مطمئن شوم؟</h2><p>کد VIN، کد فنی یا تصویر قطعه را برای کارشناسان ما ارسال کنید.</p><h2>آیا اصالت کالا تضمین می‌شود؟</h2><p>اصالت و سلامت قطعات پیش از ارسال بررسی می‌شود.</p>", "سؤالات متداول خرید قطعات پژو ۲۰۰۸", "پاسخ سؤالات رایج درباره اصالت، سازگاری و ارسال قطعات پژو ۲۰۰۸."),
            new PageSeed("ارسال و بازگشت کالا", "shipping-returns", "<h2>ارسال سفارش</h2><p>روش، زمان و هزینه ارسال پس از تأیید موجودی و مقصد با خریدار هماهنگ می‌شود. سفارش با بسته‌بندی ایمن و کد رهگیری تحویل شرکت حمل می‌شود.</p><h2>درخواست بازگشت</h2><p>در صورت مغایرت کالای دریافتی با سفارش یا آسیب در حمل، پیش از نصب قطعه و حداکثر تا ۲۴ ساعت با پشتیبانی تماس بگیرید. قطعه نصب‌شده، مخدوش یا فاقد بسته‌بندی اولیه تنها در صورت احراز ایراد فنی قابل بررسی است.</p>", "شرایط ارسال و بازگشت قطعات | mr2008.ir", "روش ارسال، پیگیری و شرایط درخواست بازگشت قطعات خریداری‌شده از mr2008.ir."),
            new PageSeed("حریم خصوصی", "privacy", "<h2>اطلاعاتی که دریافت می‌کنیم</h2><p>نام، شماره تماس و نشانی فقط برای بررسی فنی، پردازش، ارسال و پشتیبانی سفارش استفاده می‌شود.</p><h2>حفاظت از اطلاعات</h2><p>اطلاعات مشتری بدون الزام قانونی یا نیاز عملیاتی ارسال، در اختیار اشخاص نامرتبط قرار نمی‌گیرد. برای درخواست اصلاح یا حذف اطلاعات با پشتیبانی تماس بگیرید.</p>", "حریم خصوصی مشتریان | mr2008.ir", "سیاست حریم خصوصی و نحوه استفاده از اطلاعات مشتریان فروشگاه mr2008.ir."),
            new PageSeed("شرایط خرید", "terms", "<h2>سازگاری و موجودی</h2><p>مشخصات فنی و سازگاری ثبت‌شده راهنمای انتخاب است. برای قطعات حساس، تطبیق VIN پیش از نصب توصیه می‌شود.</p><h2>قیمت و پرداخت</h2><p>پرداخت سفارش آنلاین فقط از درگاه رسمی امن متصل به mr2008.ir انجام می‌شود. سفارش پس از تأیید درگاه وارد مرحله بررسی و آماده‌سازی خواهد شد.</p>", "شرایط خرید از mr2008.ir", "شرایط ثبت سفارش، تأیید فنی، قیمت، پرداخت و ارسال قطعات پژو ۲۰۰۸.")
        };
        var slugs = seeds.Select(seed => seed.Slug).ToArray();
        var existing = await dbContext.Pages.Where(item => slugs.Contains(item.Slug)).ToDictionaryAsync(item => item.Slug);
        foreach (var seed in seeds)
        {
            if (!existing.TryGetValue(seed.Slug, out var page))
            {
                page = new Page(seed.Title, seed.Slug, seed.Content, null, seed.MetaTitle, seed.MetaDescription);
                dbContext.Pages.Add(page);
            }
            else page.UpdateContent(seed.Title, seed.Content, null, seed.MetaTitle, seed.MetaDescription);
            page.Publish();
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedArticlesAsync(AtelierDbContext dbContext)
    {
        var seeds = new[]
        {
            new ArticleSeed("راهنمای تشخیص قطعه اصلی پژو ۲۰۰۸", "peugeot-2008-genuine-parts-guide", "چطور پیش از خرید، اصالت قطعه را دقیق‌تر بررسی کنیم؟", "<p>بسته‌بندی، کد فنی، کیفیت ساخت و اعتبار فروشنده چهار نشانه مهم هستند. کد قطعه باید با مشخصات خودرو تطبیق داده شود و تنها ظاهر مشابه برای تأیید سازگاری کافی نیست.</p>", "تشخیص قطعه اصلی پژو ۲۰۰۸", "راهنمای بررسی اصالت و کد فنی قطعات پژو ۲۰۰۸ پیش از خرید."),
            new ArticleSeed("فیلترهای پژو ۲۰۰۸ چه زمانی تعویض شوند؟", "peugeot-2008-filter-replacement-guide", "زمان مناسب تعویض فیلتر روغن، هوا و کابین را بشناسید.", "<p>فاصله تعویض به شرایط رانندگی، کیفیت هوا و برنامه سرویس خودرو بستگی دارد. در ترافیک و هوای آلوده، بازدید زودتر فیلترها منطقی است.</p>", "زمان تعویض فیلتر پژو ۲۰۰۸", "راهنمای زمان بررسی و تعویض فیلتر روغن، هوا و کابین پژو ۲۰۰۸."),
            new ArticleSeed("راهنمای خرید لنت ترمز پژو ۲۰۰۸", "peugeot-2008-brake-pad-buying-guide", "برای انتخاب لنت مناسب فقط به قیمت نگاه نکنید.", "<p>برند، ترکیب مواد، سازگاری با کالیپر و اصالت کالا بر عملکرد ترمز اثر دارند. پیش از سفارش، کد فنی قطعه را بررسی کنید.</p>", "خرید لنت ترمز پژو ۲۰۰۸", "نکات مهم انتخاب و خرید لنت ترمز سازگار با پژو ۲۰۰۸.")
        };
        var slugs = seeds.Select(seed => seed.Slug).ToArray();
        var existing = await dbContext.BlogPosts.Where(item => slugs.Contains(item.Slug)).ToDictionaryAsync(item => item.Slug);
        foreach (var seed in seeds)
        {
            if (!existing.TryGetValue(seed.Slug, out var post))
            {
                post = new BlogPost(seed.Title, seed.Slug, seed.Excerpt, seed.Content, null, seed.MetaTitle, seed.MetaDescription);
                dbContext.BlogPosts.Add(post);
            }
            else post.UpdateDetails(seed.Title, seed.Slug, seed.Excerpt, seed.Content, null, seed.MetaTitle, seed.MetaDescription);
            if (!post.PublishedAt.HasValue) post.Publish();
        }
        await dbContext.SaveChangesAsync();
    }

    private sealed record MediaSeed(string Slug, string Url, string Title, string FileName);
    private sealed record ProductSeed(string Name, string Slug, string Description, decimal Price, string MediaSlug,
        string? Brand, string[] CategorySlugs, bool RequiresVinCheck);
    private sealed record VehicleSeed(string Make, string Model, int YearFrom, int? YearTo, string Engine, string Trim, string Slug);
    private sealed record PageSeed(string Title, string Slug, string Content, string MetaTitle, string MetaDescription);
    private sealed record ArticleSeed(string Title, string Slug, string Excerpt, string Content, string MetaTitle, string MetaDescription);
}
