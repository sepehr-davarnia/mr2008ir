using Microsoft.EntityFrameworkCore;

namespace Atelier.Infrastructure.Data.Seeding;

public static class AtelierDbSeeder
{
    public static async Task SeedAsync(AtelierDbContext dbContext)
    {
        await SeedCategoriesAsync(dbContext);
        var mediaBySlug = await SeedMediaAsync(dbContext);
        await SeedCategoryMediaAsync(dbContext, mediaBySlug);
        await SeedSiteSettingsAsync(dbContext, mediaBySlug);
        await SeedPagesAsync(dbContext, mediaBySlug);
        await SeedBlogPostsAsync(dbContext, mediaBySlug);
        await SeedProjectsAsync(dbContext, mediaBySlug);
        await SeedProductsAsync(dbContext, mediaBySlug);
        await SeedLeadsAsync(dbContext);
    }

    private static async Task SeedSiteSettingsAsync(
        AtelierDbContext dbContext,
        IReadOnlyDictionary<string, Media> mediaBySlug)
    {
        var settings = await dbContext.SiteSettings
            .OrderBy(item => item.Id)
            .ToListAsync();

        var primary = settings.Count == 0
            ? new SiteSetting(5120)
            : settings[0];

        if (settings.Count == 0)
        {
            dbContext.SiteSettings.Add(primary);
        }

        primary.UpdateContactInfo(
            "وودزیلا",
            "تهران، خیابان ولیعصر، بالاتر از پارک ساعی، پلاک ۱۲۰",
            "021-88990011",
            "0912-123-4567",
            "https://wa.me/989121234567",
            "https://instagram.com/woodzilla.ir",
            "https://t.me/woodzilla",
            "info@woodzilla.ir");

        mediaBySlug.TryGetValue("homepage-hero", out var heroMedia);
        mediaBySlug.TryGetValue("homepage-secondary", out var secondaryMedia);
        mediaBySlug.TryGetValue("category-default", out var defaultCategoryMedia);

        primary.UpdateVisualMedia(heroMedia?.Id, secondaryMedia?.Id, defaultCategoryMedia?.Id);

        if (settings.Count > 1)
        {
            dbContext.SiteSettings.RemoveRange(settings.Skip(1));
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(AtelierDbContext dbContext)
    {
        var seeds = new[]
        {
            new CategorySeed("ترمووود", "thermowood", null),
            new CategorySeed("تایل ترمووود", "thermowood-tiles", null),
            new CategorySeed("تایل چمن مصنوعی", "artificial-grass-tiles", null),
            new CategorySeed("ترمووال", "thermowall", null),
            new CategorySeed("چوب روسی", "russian-wood", null),
            new CategorySeed("لمبه", "lambe", null)
        };

        var seedSlugs = seeds
            .Select(seed => GenerateSlug(seed.Slug))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingCategories = await dbContext.Categories.ToListAsync();

        foreach (var category in existingCategories.Where(category => !seedSlugs.Contains(category.Slug)))
        {
            category.SetParent(null);
        }

        await dbContext.SaveChangesAsync();

        dbContext.Categories.RemoveRange(existingCategories.Where(category => !seedSlugs.Contains(category.Slug)));
        await dbContext.SaveChangesAsync();

        existingCategories = await dbContext.Categories.ToListAsync();
        var categoryMap = existingCategories.ToDictionary(category => category.Slug, category => category, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in seeds)
        {
            var normalizedSlug = GenerateSlug(seed.Slug);
            categoryMap.TryGetValue(normalizedSlug, out var category);
            Category? parent = null;

            if (!string.IsNullOrWhiteSpace(seed.ParentSlug))
            {
                var normalizedParent = GenerateSlug(seed.ParentSlug);
                categoryMap.TryGetValue(normalizedParent, out parent);
            }

            if (category is null)
            {
                category = new Category(seed.Name, normalizedSlug, parent);
                dbContext.Categories.Add(category);
                categoryMap[normalizedSlug] = category;
                continue;
            }

            if (!string.Equals(category.Name, seed.Name, StringComparison.Ordinal))
            {
                category.UpdateName(seed.Name);
            }

            category.SetParent(parent);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, Media>> SeedMediaAsync(AtelierDbContext dbContext)
    {
        var seeds = new[]
        {
            new MediaSeed(
                "homepage-hero",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28?auto=format&fit=crop&w=1800&q=80",
                "نمای ترمووود معاصر",
                "نمای چوبی ترمووود مدرن با الوار عمودی",
                "thermowood-facade-modern.jpg",
                "هیرو",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28",
                true),
            new MediaSeed(
                "homepage-secondary",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1400&q=80",
                "فضای داخلی چوبی",
                "فضای داخلی با دیوارپوش چوبی و نور گرم",
                "warm-wood-interior-wall.jpg",
                "هیرو",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511",
                true),
            new MediaSeed(
                "category-default",
                "https://images.unsplash.com/photo-1487017159836-4e23ece2e4cf?auto=format&fit=crop&w=1200&q=80",
                "چوب های معماری",
                "چوب های معماری با پرداخت حرفه ای برای پروژه ها",
                "architectural-wood-selection.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1487017159836-4e23ece2e4cf",
                true),
            new MediaSeed(
                "category-wood",
                "https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&w=1400&q=80",
                "چوب طبیعی",
                "انباشت الوار چوب طبیعی آماده فرآوری",
                "solid-wood-planks-storage.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1503387762-592deb58ef4e",
                true),
            new MediaSeed(
                "category-thermowood",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1200&q=80",
                "ترمووود مقاوم",
                "ترمووود مقاوم در برابر رطوبت و تابش آفتاب",
                "thermowood-cladding-showcase.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                true),
            new MediaSeed(
                "category-applications",
                "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1400&q=80",
                "کاربردهای معماری چوب",
                "تراس چوبی با ترمووود در فضای باز",
                "outdoor-thermowood-terrace.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85",
                true),
            new MediaSeed(
                "thermowood-decking",
                "https://images.unsplash.com/photo-1471879832106-c7ab9e0cee23?auto=format&fit=crop&w=1400&q=80",
                "ترمووود کف فضای باز",
                "ترمووود کف فضای باز برای محوطه و تراس",
                "thermowood-decking-detail.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1471879832106-c7ab9e0cee23",
                true),
            new MediaSeed(
                "thermowood-facade",
                "https://images.unsplash.com/photo-1475856034135-7d576baaec70?auto=format&fit=crop&w=1400&q=80",
                "ترمووود نما",
                "نمای مدرن با ترمووود مقاوم در برابر آب و آفتاب",
                "thermowood-facade-lines.jpg",
                "بلاگ",
                "https://images.unsplash.com/photo-1475856034135-7d576baaec70",
                true),
            new MediaSeed(
                "thermowood-pergola",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1400&q=80",
                "پرگولا ترمووود",
                "پرگولا ترمووود برای فضای باز و محوطه",
                "thermowood-pergola-structure.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                true),
            new MediaSeed(
                "natural-wood-spruce",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "چوب روسی یولکا",
                "چوب روسی یولکا مناسب سازه و دکوراسیون داخلی",
                "russian-spruce-lumber.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "imported-wood-iroko",
                "https://images.unsplash.com/photo-1501183638710-841dd1904471?auto=format&fit=crop&w=1400&q=80",
                "چوب وارداتی ایروکو",
                "چوب ایروکو وارداتی مناسب فضای خارجی",
                "iroko-wood-texture.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501183638710-841dd1904471",
                true),
            new MediaSeed(
                "interior-cladding",
                "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1400&q=80",
                "دیوارپوش ترمووود",
                "دیوارپوش ترمووود برای طراحی داخلی لوکس",
                "thermowood-interior-wall.jpg",
                "بلاگ",
                "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
                true),
            new MediaSeed(
                "thermowood-guide",
                "https://images.unsplash.com/photo-1504384308090-c894fdcc538d?auto=format&fit=crop&w=1400&q=80",
                "راهنمای ترمووود",
                "جزئیات سطح ترمووود برای راهنمای انتخاب",
                "thermowood-guide-detail.jpg",
                "بلاگ",
                "https://images.unsplash.com/photo-1504384308090-c894fdcc538d",
                true),
            new MediaSeed(
                "company-workshop",
                "https://images.unsplash.com/photo-1487017159836-4e23ece2e4cf?auto=format&fit=crop&w=1400&q=80",
                "کارگاه ترمووود",
                "کارگاه تولید و فرآوری ترمووود",
                "thermowood-workshop-process.jpg",
                "بلاگ",
                "https://images.unsplash.com/photo-1487017159836-4e23ece2e4cf",
                true),
            new MediaSeed(
                "category-natural-wood",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=1400&q=80",
                "چوب طبیعی برای دکوراسیون",
                "تخته های چوب طبیعی مناسب دکوراسیون داخلی",
                "natural-wood-deck-boards.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6",
                true),
            new MediaSeed(
                "category-imported-wood",
                "https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&w=1400&q=80",
                "چوب وارداتی لوکس",
                "چوب وارداتی با پرداخت لوکس برای پروژه های خاص",
                "imported-wood-luxury.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1503387762-592deb58ef4e",
                true),
            new MediaSeed(
                "category-thermowood-decking",
                "https://images.unsplash.com/photo-1501127122-f385ca6ddd9d?auto=format&fit=crop&w=1400&q=80",
                "کفپوش ترمووود",
                "کفپوش ترمووود مناسب تراس و محوطه",
                "thermowood-decking-category.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1501127122-f385ca6ddd9d",
                true),
            new MediaSeed(
                "category-thermowood-facade",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28?auto=format&fit=crop&w=1400&q=80",
                "نمای ترمووود",
                "نمای چوبی ترمووود با اجرای مدرن",
                "thermowood-facade-category.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28",
                true),
            new MediaSeed(
                "category-thermowood-pergola",
                "https://images.unsplash.com/photo-1469796466635-455ede028aca?auto=format&fit=crop&w=1400&q=80",
                "پرگولای ترمووود",
                "پرگولای چوبی برای فضاهای باز",
                "thermowood-pergola-category.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1469796466635-455ede028aca",
                true),
            new MediaSeed(
                "category-interior",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1400&q=80",
                "فضای داخلی چوبی",
                "طراحی داخلی با دیوارپوش چوبی",
                "wood-interior-cladding.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511",
                true),
            new MediaSeed(
                "category-exterior",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1400&q=80",
                "فضای خارجی با ترمووود",
                "اجرای ترمووود در فضای خارجی",
                "thermowood-exterior-detail.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                true),
            new MediaSeed(
                "category-roof",
                "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1400&q=80",
                "روف گاردن چوبی",
                "روف گاردن با کفپوش چوبی مقاوم",
                "thermowood-roof-garden.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
                true),
            new MediaSeed(
                "category-wall-cladding",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28?auto=format&fit=crop&w=1400&q=80",
                "دیوارپوش چوبی",
                "دیوارپوش چوبی برای نما و فضای داخلی",
                "wood-wall-cladding.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28",
                true),
            new MediaSeed(
                "page-about-us-hero",
                "https://images.unsplash.com/photo-1462556791646-c201b8241a94?auto=format&fit=crop&w=1800&q=80",
                "تیم وودزیلا",
                "تیم تولید و مشاوره وودزیلا در کنار محصولات چوبی",
                "woodzilla-team-collaboration.jpg",
                "صفحه",
                "https://images.unsplash.com/photo-1462556791646-c201b8241a94",
                true),
            new MediaSeed(
                "page-contact-hero",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1800&q=80",
                "ارتباط با وودزیلا",
                "تصویر فضایی گرم برای صفحه تماس با ما",
                "wood-contact-warm-space.jpg",
                "صفحه",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                true),
            new MediaSeed(
                "page-services-hero",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1800&q=80",
                "خدمات تخصصی چوب",
                "خدمات اجرا و طراحی با چوب طبیعی و ترمووود",
                "wood-services-hero.jpg",
                "صفحه",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511",
                true),
            new MediaSeed(
                "page-faq-hero",
                "https://images.unsplash.com/photo-1501183638710-841dd1904471?auto=format&fit=crop&w=1800&q=80",
                "پرسش های متداول",
                "نمای نزدیک از چوب برای صفحه سوالات متداول",
                "faq-wood-texture.jpg",
                "صفحه",
                "https://images.unsplash.com/photo-1501183638710-841dd1904471",
                true),
            new MediaSeed(
                "blog-thermowood-what-is",
                "https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&w=1400&q=80",
                "فرآیند تولید ترمووود",
                "تصویر مرتبط با فرآیند تولید ترمووود",
                "thermowood-production-process.jpg",
                "بلاگ",
                "https://images.unsplash.com/photo-1503387762-592deb58ef4e",
                true),
            new MediaSeed(
                "blog-thermowood-vs-regular",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=1400&q=80",
                "مقایسه ترمووود و چوب معمولی",
                "نمونه چوب برای مقایسه ترمووود و چوب معمولی",
                "thermowood-vs-regular-wood.jpg",
                "بلاگ",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6",
                true),
            new MediaSeed(
                "blog-thermowood-facade",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28?auto=format&fit=crop&w=1400&q=80",
                "نمای ترمووود مدرن",
                "نمای مدرن ترمووود برای پروژه های معماری",
                "modern-thermowood-facade.jpg",
                "بلاگ",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28",
                true),
            new MediaSeed(
                "blog-thermowood-maintenance",
                "https://images.unsplash.com/photo-1501127122-f385ca6ddd9d?auto=format&fit=crop&w=1400&q=80",
                "نگهداری ترمووود",
                "کفپوش ترمووود مناسب نگهداری دوره ای",
                "thermowood-maintenance-deck.jpg",
                "بلاگ",
                "https://images.unsplash.com/photo-1501127122-f385ca6ddd9d",
                true),
            new MediaSeed(
                "blog-finnish-thermowood",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "ترمووود فنلاندی",
                "چوب ترمووود فنلاندی با بافت طبیعی",
                "finnish-thermowood-texture.jpg",
                "بلاگ",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "project-north-villa",
                "https://images.unsplash.com/photo-1504384308090-c894fdcc538d?auto=format&fit=crop&w=1600&q=80",
                "پروژه نمای ویلای شمال",
                "نمای چوبی اجرا شده برای ویلای شمال",
                "thermowood-villa-facade.jpg",
                "پروژه",
                "https://images.unsplash.com/photo-1504384308090-c894fdcc538d",
                true),
            new MediaSeed(
                "project-terrace-decking",
                "https://images.unsplash.com/photo-1471879832106-c7ab9e0cee23?auto=format&fit=crop&w=1600&q=80",
                "پروژه کف تراس",
                "کف ترمووود اجرا شده در تراس مجتمع مسکونی",
                "thermowood-terrace-project.jpg",
                "پروژه",
                "https://images.unsplash.com/photo-1471879832106-c7ab9e0cee23",
                true),
            new MediaSeed(
                "project-garden-pergola",
                "https://images.unsplash.com/photo-1469796466635-455ede028aca?auto=format&fit=crop&w=1600&q=80",
                "پروژه پرگولای باغ",
                "پرگولای چوبی اجرا شده در محوطه باغ",
                "garden-pergola-thermowood.jpg",
                "پروژه",
                "https://images.unsplash.com/photo-1469796466635-455ede028aca",
                true),
            new MediaSeed(
                "project-north-villa-terrace",
                "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1600&q=80",
                "تراس پروژه ویلای شمال",
                "جزئیات تراس چوبی پروژه ویلای شمال",
                "villa-terrace-thermowood.jpg",
                "پروژه",
                "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85",
                true),
            new MediaSeed(
                "project-terrace-decking-night",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1600&q=80",
                "نورپردازی کف ترمووود",
                "کف ترمووود محوطه در شب با نورپردازی گرم",
                "thermowood-deck-night-lighting.jpg",
                "پروژه",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                true),
            new MediaSeed(
                "project-garden-pergola-closeup",
                "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1600&q=80",
                "نمای نزدیک پرگولا",
                "جزئیات بافت چوب در پرگولای فضای سبز",
                "pergola-wood-detail.jpg",
                "پروژه",
                "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
                true),
            new MediaSeed(
                "product-finnish-thermowood-19mm",
                "https://images.unsplash.com/photo-1501183638710-841dd1904471?auto=format&fit=crop&w=1400&q=80",
                "ترمووود فنلاندی ۱۹ میلی متر",
                "تخته ترمووود فنلاندی برای نما و کف",
                "finnish-thermowood-19mm.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501183638710-841dd1904471",
                true),
            new MediaSeed(
                "product-thermowood-facade-shp",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28?auto=format&fit=crop&w=1400&q=80",
                "ترمووود نما SHP",
                "پروفیل ترمووود SHP مناسب نمای مدرن",
                "thermowood-facade-shp.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28",
                true),
            new MediaSeed(
                "product-russian-spruce",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "چوب روسی یولکا",
                "چوب روسی یولکا با بافت روشن",
                "russian-spruce-product.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-thermowood-decking",
                "https://images.unsplash.com/photo-1501127122-f385ca6ddd9d?auto=format&fit=crop&w=1400&q=80",
                "ترمووود کف فضای باز",
                "کفپوش ترمووود مناسب محوطه و تراس",
                "thermowood-decking-product.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501127122-f385ca6ddd9d",
                true),
            new MediaSeed(
                "product-thermowood-pergola",
                "https://images.unsplash.com/photo-1469796466635-455ede028aca?auto=format&fit=crop&w=1400&q=80",
                "ترمووود پرگولا",
                "چوب ترمووود مناسب اجرای پرگولا",
                "thermowood-pergola-product.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1469796466635-455ede028aca",
                true),
            new MediaSeed(
                "product-thermowood-interior",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1400&q=80",
                "لمبه ترمووود داخلی",
                "لمبه ترمووود برای دیوارپوش داخلی",
                "thermowood-interior-lamella.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511",
                true),
            new MediaSeed(
                "product-thermowood-ash",
                "https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&w=1400&q=80",
                "چوب اش ترمووود",
                "چوب اش ترمووود با بافت یکنواخت",
                "thermowood-ash-product.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1503387762-592deb58ef4e",
                true),
            new MediaSeed(
                "product-thermowood-pine-utv",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1400&q=80",
                "ترمووود کاج فنلاندی UTV",
                "پروفیل UTV ترمووود برای نمای خطی",
                "thermowood-pine-utv-profile.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                true),
            new MediaSeed(
                "product-thermowood-facade-uts",
                "https://images.unsplash.com/photo-1475856034135-7d576baaec70?auto=format&fit=crop&w=1400&q=80",
                "ترمووود نمای مدرن UTS",
                "پروفیل UTS ترمووود برای نمای یکدست",
                "thermowood-uts-profile.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1475856034135-7d576baaec70",
                true),
            new MediaSeed(
                "product-imported-iroko",
                "https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&w=1400&q=80",
                "چوب وارداتی ایروکو",
                "چوب ایروکو مناسب فضای خارجی",
                "imported-iroko-board.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1503387762-592deb58ef4e",
                true),
            new MediaSeed(
                "product-russian-pine-kiln",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=1400&q=80",
                "چوب روسی نراد خشک",
                "چوب روسی نراد خشک شده مناسب نجاری",
                "russian-pine-kiln-dried.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6",
                true),
            new MediaSeed(
                "product-thermowood-roof-garden",
                "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1400&q=80",
                "ترمووود روف گاردن",
                "ترمووود مناسب روف گاردن و تراس",
                "thermowood-roof-garden-board.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
                true),
            new MediaSeed(
                "category-thermowood-main",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28?auto=format&fit=crop&w=1400&q=80",
                "دسته ترمووود",
                "نمای تخته های ترمووود برای نما و دیوارپوش",
                "category-thermowood-main.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28",
                true),
            new MediaSeed(
                "category-thermowood-tiles",
                "https://images.unsplash.com/photo-1471879832106-c7ab9e0cee23?auto=format&fit=crop&w=1400&q=80",
                "دسته تایل ترمووود",
                "تایل ترمووود مشبک روی زیرسازی منظم",
                "category-thermowood-tiles.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1471879832106-c7ab9e0cee23",
                true),
            new MediaSeed(
                "category-artificial-grass-tiles",
                "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1400&q=80",
                "دسته تایل چمن مصنوعی",
                "تایل چمن مصنوعی ماژولار در محوطه چوبی",
                "category-artificial-grass-tiles.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85",
                true),
            new MediaSeed(
                "category-thermowall",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28?auto=format&fit=crop&w=1400&q=80",
                "دسته ترمووال",
                "پوشش دیواری ترمووود با خطوط افقی",
                "category-thermowall.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28",
                true),
            new MediaSeed(
                "category-russian-wood",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "دسته چوب روسی",
                "تخته های چوب روسی خشک شده و مرتب",
                "category-russian-wood.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "category-lambe",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1400&q=80",
                "دسته لمبه",
                "لمبه چوبی نصب شده در فضای داخلی روشن",
                "category-lambe.jpg",
                "دسته‌بندی",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511",
                true),
            new MediaSeed(
                "product-thermowood-simple-19mm-42mm",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1400&q=80",
                "ترمووود گره ساده ۱۹ میلیمتری",
                "ترمووود ضخامت ۱۹ میلیمتر عرض ۴.۲ سانت برای نما و دیوار",
                "thermowood-simple-19mm-42mm.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                true),
            new MediaSeed(
                "product-thermowood-simple-16mm",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1400&q=80",
                "ترمووود گره ساده ۱۶ میلیمتری",
                "پروفیل ترمووود گره ساده ۱۶ میلیمتر برای دیوارپوش سبک",
                "thermowood-simple-16mm.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                true),
            new MediaSeed(
                "product-thermowood-simple-14mm",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1400&q=80",
                "ترمووود گره ساده ۱۴ میلیمتری",
                "ترمووود ۱۴ میلیمتری مناسب پوشش دیوار و سقف",
                "thermowood-simple-14mm.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                true),
            new MediaSeed(
                "product-thermowood-simple-8mm",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1400&q=80",
                "ترمووود گره ساده ۸ میلیمتری",
                "ترمووود نازک ۸ میلیمتری برای روکش های سبک",
                "thermowood-simple-8mm.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                true),
            new MediaSeed(
                "product-thermowood-parvaneh-14mm",
                "https://images.unsplash.com/photo-1469796466635-455ede028aca?auto=format&fit=crop&w=1400&q=80",
                "ترمووود طرح پروانه ۱۴ میلیمتری",
                "ترمووود پروانه ای ۱۴ میلیمتری با بافت منظم برای نما",
                "thermowood-parvaneh-14mm.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1469796466635-455ede028aca",
                true),
            new MediaSeed(
                "product-thermowood-tile-standard",
                "https://images.unsplash.com/photo-1501127122-f385ca6ddd9d?auto=format&fit=crop&w=1400&q=80",
                "تایل ترمووود استاندارد",
                "تایل ترمووود استاندارد برای کف تراس",
                "thermowood-tile-standard.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501127122-f385ca6ddd9d",
                true),
            new MediaSeed(
                "product-thermowood-tile-puzzle",
                "https://images.unsplash.com/photo-1471879832106-c7ab9e0cee23?auto=format&fit=crop&w=1400&q=80",
                "تایل ترمووود پازلی",
                "تایل ترمووود پازلی با اتصال آسان",
                "thermowood-tile-puzzle.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1471879832106-c7ab9e0cee23",
                true),
            new MediaSeed(
                "product-artificial-grass-tile",
                "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1400&q=80",
                "تایل چمن مصنوعی",
                "تایل چمن مصنوعی روی کف چوبی منظم",
                "artificial-grass-tile.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85",
                true),
            new MediaSeed(
                "product-thermowall-panel",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28?auto=format&fit=crop&w=1400&q=80",
                "پنل ترمووال",
                "پنل ترمووال شیار دار برای پوشش دیوار",
                "thermowall-panel.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28",
                true),
            new MediaSeed(
                "product-russian-wood-10x5-3m",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۱۰×۵ طول ۳ متر",
                "تخته روسی ۱۰ در ۵ سانتی‌متر به طول ۳ متر برای زیرسازی",
                "russian-wood-10x5-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-russian-wood-10x2-3m",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۱۰×۲ طول ۳ متر",
                "تخته روسی ۱۰ در ۲ سانتی‌متر برای کلاف بندی سبک",
                "russian-wood-10x2-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6",
                true),
            new MediaSeed(
                "product-russian-wood-10x15-3m",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۱۰×۱.۵ طول ۳ متر",
                "تخته روسی ۱۰ در ۱.۵ سانتی‌متر برای روکوب و کلاف سبک",
                "russian-wood-10x15-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6",
                true),
            new MediaSeed(
                "product-russian-wood-10x1-3m",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۱۰×۱ طول ۳ متر",
                "تخته روسی ۱۰ در ۱ سانتی‌متر برای روکار و سازه های سبک",
                "russian-wood-10x1-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6",
                true),
            new MediaSeed(
                "product-russian-wood-15x15-3m",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۱۵×۱.۵ طول ۳ متر",
                "تخته روسی ۱۵ در ۱.۵ سانتی‌متر برای زیرسازی و قاب",
                "russian-wood-15x15-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-russian-wood-15x2-3m",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۱۵×۲ طول ۳ متر",
                "تخته روسی ۱۵ در ۲ سانتی‌متر برای تیرک و زیرسازی",
                "russian-wood-15x2-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-russian-wood-5x3-3m",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۵×۳ طول ۳ متر",
                "تخته روسی ۵ در ۳ سانتی‌متر مناسب کلاف بندی و شاسی کشی",
                "russian-wood-5x3-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-russian-wood-20x2-3m",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۲۰×۲ طول ۳ متر",
                "تخته روسی ۲۰ در ۲ سانتی‌متر برای تیر و لمبه زیرسازی",
                "russian-wood-20x2-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6",
                true),
            new MediaSeed(
                "product-russian-wood-15x1-3m",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۱۵×۱ طول ۳ متر",
                "تخته روسی ۱۵ در ۱ سانتی‌متر برای روکوب و دکور",
                "russian-wood-15x1-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6",
                true),
            new MediaSeed(
                "product-russian-wood-15x5-3m",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۱۵×۵ طول ۳ متر",
                "تخته روسی ۱۵ در ۵ سانتی‌متر مناسب تیر و کلاف",
                "russian-wood-15x5-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-russian-wood-20x5-3m",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "تخته روسی ۲۰×۵ طول ۳ متر",
                "تخته روسی ۲۰ در ۵ سانتی‌متر برای زیرسازی و تیرک",
                "russian-wood-20x5-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-russian-bench-10x08-3m",
                "https://images.unsplash.com/photo-1501183638710-841dd1904471?auto=format&fit=crop&w=1400&q=80",
                "چوب نیمکتی روسی ۱۰×۰.۸ طول ۳ متر",
                "پروفیل نیمکتی روسی با لبه نرم برای مبلمان فضای باز",
                "russian-bench-10x08-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501183638710-841dd1904471",
                true),
            new MediaSeed(
                "product-russian-bench-10x07-3m",
                "https://images.unsplash.com/photo-1501183638710-841dd1904471?auto=format&fit=crop&w=1400&q=80",
                "چوب نیمکتی روسی ۱۰×۰.۷ طول ۳ متر",
                "چوب نیمکتی روسی با ضخامت ۰.۷ مناسب نشیمن و تکیه گاه",
                "russian-bench-10x07-3m.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501183638710-841dd1904471",
                true),
            new MediaSeed(
                "product-russian-angle-5cm",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "نبشی روسی ۵ سانتی‌متر",
                "نبشی روسی ۵ سانتی‌متری برای قاب و مهار اتصالات",
                "russian-angle-5cm.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-russian-angle-3cm",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "نبشی روسی ۳ سانتی‌متر",
                "نبشی روسی ۳ سانتی‌متری برای پوشش لبه و گوشه",
                "russian-angle-3cm.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-russian-wood-strip",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=1400&q=80",
                "فتیله چوب روسی",
                "فتیله چوب روسی برای آب بندی و درزپوشی",
                "russian-wood-strip.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1501004318641-b39e6451bec6",
                true),
            new MediaSeed(
                "product-subwood-3x2",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "چوب زیرکار ۳×۲",
                "چوب زیرکار ۳ در ۲ سانتی‌متر برای شبکه بندی دقیق",
                "subwood-3x2.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-subwood-4x2",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "چوب زیرکار ۴×۲",
                "چوب زیرکار ۴ در ۲ سانتی‌متر برای زیرسازی کف و نما",
                "subwood-4x2.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-subwood-5x3",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "چوب زیرکار ۵×۳",
                "چوب زیرکار ۵ در ۳ سانتی‌متر برای تحمل بار بالاتر",
                "subwood-5x3.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-subwood-4x6",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "چوب زیرکار ۴×۶",
                "چوب زیرکار ۴ در ۶ سانتی‌متر برای تیرک و مهار سازه",
                "subwood-4x6.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-subwood-5x5",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?auto=format&fit=crop&w=1400&q=80",
                "چوب زیرکار ۵×۵",
                "چوب زیرکار ۵ در ۵ سانتی‌متر برای شبکه های مقاوم",
                "subwood-5x5.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e",
                true),
            new MediaSeed(
                "product-lambe-wood",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1400&q=80",
                "چوب لمبه",
                "لمبه چوبی برای دیوارپوش و سقف داخلی",
                "lambe-wood.jpg",
                "محصول",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511",
                true),
            new MediaSeed(
                "service-thermowood-facade-installation",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28?auto=format&fit=crop&w=1400&q=80",
                "اجرای ترمووود نما",
                "نمای ترمووود با اجرا و درز بندی دقیق",
                "thermowood-facade-installation.jpg",
                "خدمات",
                "https://images.unsplash.com/photo-1502005097973-6a7082348e28",
                true),
            new MediaSeed(
                "service-thermowood-wall-installation",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1400&q=80",
                "اجرای ترمووود دیوار",
                "دیوارپوش ترمووود با نصب منظم در فضای داخلی",
                "thermowood-wall-installation.jpg",
                "خدمات",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511",
                true),
            new MediaSeed(
                "service-thermowood-deck-installation",
                "https://images.unsplash.com/photo-1501127122-f385ca6ddd9d?auto=format&fit=crop&w=1400&q=80",
                "اجرای ترمووود تراس",
                "اجرای کف ترمووود تراس با فواصل منظم و آبرو",
                "thermowood-deck-installation.jpg",
                "خدمات",
                "https://images.unsplash.com/photo-1501127122-f385ca6ddd9d",
                true),
            new MediaSeed(
                "service-thermowood-ceiling-installation",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1400&q=80",
                "اجرای ترمووود سقف",
                "سقف ترمووود خطی با نورپردازی مخفی",
                "thermowood-ceiling-installation.jpg",
                "خدمات",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511",
                true)
        };

        var slugs = seeds.Select(seed => seed.Slug).ToList();
        var existing = await dbContext.Media
            .Where(media => slugs.Contains(media.Slug))
            .ToDictionaryAsync(media => media.Slug, media => media, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in seeds)
        {
            if (existing.TryGetValue(seed.Slug, out var media))
            {
                media.UpdateDetails(
                    seed.Url,
                    seed.Title,
                    seed.AltText,
                    seed.FileName,
                    seed.Purpose,
                    seed.SourceUrl,
                    seed.IsNotDownloaded);
                continue;
            }

            dbContext.Media.Add(new Media(
                seed.Slug,
                seed.Url,
                null,
                "image/jpeg",
                null,
                seed.Title,
                seed.AltText,
                seed.FileName,
                seed.Purpose,
                seed.SourceUrl,
                seed.IsNotDownloaded));
        }

        await dbContext.SaveChangesAsync();

        return await dbContext.Media
            .Where(media => slugs.Contains(media.Slug))
            .ToDictionaryAsync(media => media.Slug);
    }

    private static async Task SeedPagesAsync(
        AtelierDbContext dbContext,
        IReadOnlyDictionary<string, Media> mediaBySlug)
    {
        var seeds = new[]
        {
            new PageSeed(
                "درباره ما",
                "about-us",
                "ما در وودزیلا با تمرکز بر واردات و فرآوری چوب و ترمووود، راهکارهای تخصصی برای پروژه های ساختمانی ارائه می دهیم. تیم ما کیفیت، دوام و زیبایی را در کنار مشاوره فنی ارائه می کند.",
                "درباره ما | وودزیلا",
                "آشنایی با تیم و خدمات وودزیلا در حوزه چوب و ترمووود",
                "page-about-us-hero"),
            new PageSeed(
                "تماس با ما",
                "contact-us",
                "برای دریافت مشاوره، استعلام موجودی و ثبت سفارش با واحد فروش و پشتیبانی ما در ارتباط باشید. تیم وودزیلا آماده پاسخگویی به سوالات شماست.",
                "تماس با ما | وودزیلا",
                "راه های ارتباط با وودزیلا برای مشاوره و خرید ترمووود",
                "page-contact-hero"),
            new PageSeed(
                "خدمات",
                "services",
                "خدمات ما شامل تامین چوب وارداتی، تولید ترمووود، اجرای نما، کف و پرگولا، و ارائه دیتیل های فنی و پشتیبانی اجرایی است.",
                "خدمات وودزیلا",
                "خدمات تخصصی وودزیلا در تامین و اجرای ترمووود",
                "page-services-hero"),
            new PageSeed(
                "راهنمای ترمووود",
                "thermowood-guide",
                "در این راهنما با مزایا، استانداردها، روش های نصب و نکات نگهداری ترمووود آشنا می شوید تا بهترین انتخاب را برای پروژه خود داشته باشید.",
                "راهنمای ترمووود",
                "راهنمای کامل انتخاب و اجرای ترمووود",
                "thermowood-guide"),
            new PageSeed(
                "سوالات متداول",
                "faq",
                "پاسخ به سوالات پرتکرار درباره ترمووود، تفاوت ها با چوب معمولی، زمان تحویل و شرایط نگهداری در این بخش جمع آوری شده است.",
                "سوالات متداول ترمووود",
                "پاسخ به سوالات رایج درباره ترمووود",
                "page-faq-hero"),
            new PageSeed(
                "اجرای ترمووود نما",
                "thermowood-facade-installation",
                "اجرای نمای ترمووود با جزئیات آب‌بندی، فواصل استاندارد و مهاربندی دقیق برای دوام طولانی.",
                "اجرای ترمووود نما",
                "خدمات اجرای نمای ترمووود با دیتیل فنی و کنترل کیفیت",
                "service-thermowood-facade-installation"),
            new PageSeed(
                "اجرای ترمووود دیوار",
                "thermowood-wall-installation",
                "نصب دیوارپوش ترمووود در فضاهای داخلی و خارجی با زیرسازی تراز، اتصالات مخفی و تهویه پشت کار.",
                "اجرای ترمووود دیوار",
                "اجرای تخصصی دیوارپوش ترمووود با زیرسازی اصولی",
                "service-thermowood-wall-installation"),
            new PageSeed(
                "اجرای ترمووود تراس",
                "thermowood-deck-installation",
                "اجرای کف ترمووود تراس با شیب بندی، درز انبساط و انتخاب پروفیل متناسب با تردد.",
                "اجرای ترمووود تراس",
                "خدمات نصب کف ترمووود تراس با زیرسازی مقاوم",
                "service-thermowood-deck-installation"),
            new PageSeed(
                "اجرای ترمووود سقف",
                "thermowood-ceiling-installation",
                "اجرای سقف ترمووود خطی یا لمبه با سازه سبک، عبور تاسیسات و تهویه مناسب برای طول عمر بیشتر.",
                "اجرای ترمووود سقف",
                "اجرای سقف ترمووود با دیتیل فنی و زیرسازی دقیق",
                "service-thermowood-ceiling-installation")
        };

        foreach (var seed in seeds)
        {
            var normalizedSlug = GenerateSlug(seed.Slug);
            var page = await dbContext.Pages.FirstOrDefaultAsync(item => item.Slug == normalizedSlug);

            if (page is not null)
            {
                continue;
            }

            mediaBySlug.TryGetValue(seed.FeaturedMediaSlug, out var featuredMedia);

            page = new Page(
                seed.Title,
                normalizedSlug,
                seed.Content,
                featuredMedia?.Id,
                seed.MetaTitle,
                seed.MetaDescription);
            page.Publish();
            dbContext.Pages.Add(page);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedBlogPostsAsync(
        AtelierDbContext dbContext,
        IReadOnlyDictionary<string, Media> mediaBySlug)
    {
        var seeds = new[]
        {
            new BlogPostSeed(
                "ترمووود چیست؟",
                "thermowood-what-is-it",
                "ترمووود چوبی است که با فرآیند حرارتی پایدار شده و دوام بالاتری در برابر رطوبت و تغییرات دما دارد.",
                "ترمووود با حرارت کنترل شده و بدون مواد شیمیایی تولید می شود و پایداری ابعادی بالایی دارد. این ویژگی آن را برای نما، کف و پرگولا به انتخابی مطمئن تبدیل می کند.",
                "ترمووود چیست",
                "تعریف ترمووود و مزایای آن برای پروژه های ساختمانی",
                "blog-thermowood-what-is"),
            new BlogPostSeed(
                "تفاوت ترمووود و چوب معمولی",
                "thermowood-vs-regular-wood",
                "تفاوت اصلی در مقاومت، ثبات رنگ و دوام در فضای باز است که ترمووود را متمایز می کند.",
                "در ترمووود میزان رطوبت کاهش می یابد، جذب آب کمتر می شود و انبساط و انقباض به حداقل می رسد. این مزایا در کنار زیبایی طبیعی، هزینه نگهداری را کاهش می دهد.",
                "تفاوت ترمووود و چوب معمولی",
                "مقایسه دوام و کارایی ترمووود با چوب های معمولی",
                "blog-thermowood-vs-regular"),
            new BlogPostSeed(
                "کاربرد ترمووود در نما",
                "thermowood-facade-applications",
                "ترمووود نما انتخابی حرفه ای برای پروژه های مدرن است که در برابر آفتاب و باران دوام دارد.",
                "برای طراحی نما، انتخاب پروفیل مناسب و اجرای اصولی اهمیت زیادی دارد. ترمووود با رنگ طبیعی و مقاومت بالا، جلوه گرم و ماندگار ایجاد می کند.",
                "کاربرد ترمووود در نما",
                "مزایای استفاده از ترمووود برای نمای ساختمان",
                "blog-thermowood-facade"),
            new BlogPostSeed(
                "نگهداری ترمووود",
                "thermowood-maintenance",
                "با چند اقدام ساده مانند روغن کاری دوره ای و نظافت، عمر ترمووود را افزایش دهید.",
                "برای حفظ رنگ و دوام ترمووود، استفاده از روغن های مخصوص، نظافت منظم و رعایت نکات نصب توصیه می شود. این کارها از ترک خوردگی و تغییر رنگ جلوگیری می کند.",
                "نگهداری ترمووود",
                "نکات کلیدی نگهداری و افزایش عمر ترمووود",
                "blog-thermowood-maintenance"),
            new BlogPostSeed(
                "چرا ترمووود فنلاندی؟",
                "why-finnish-thermowood",
                "ترمووود فنلاندی به دلیل کیفیت چوب و استانداردهای تولید، انتخابی مطمئن برای پروژه های لوکس است.",
                "چوب فنلاندی با دانسیته مناسب و فرآوری استاندارد، دوام و ثبات رنگ بالایی دارد. انتخاب این نوع ترمووود برای نما و کف، ارزش افزوده پروژه را افزایش می دهد.",
                "ترمووود فنلاندی",
                "دلایل انتخاب ترمووود فنلاندی برای نما و کف",
                "blog-finnish-thermowood")
        };

        foreach (var seed in seeds)
        {
            var normalizedSlug = GenerateSlug(seed.Slug);
            var post = await dbContext.BlogPosts.FirstOrDefaultAsync(item => item.Slug == normalizedSlug);

            if (post is not null)
            {
                continue;
            }

            mediaBySlug.TryGetValue(seed.FeaturedMediaSlug, out var featuredMedia);

            post = new BlogPost(
                seed.Title,
                normalizedSlug,
                seed.Excerpt,
                seed.Content,
                featuredMedia?.Id,
                seed.MetaTitle,
                seed.MetaDescription);
            post.Publish();
            dbContext.BlogPosts.Add(post);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(
        AtelierDbContext dbContext,
        IReadOnlyDictionary<string, Media> mediaBySlug)
    {
        var seeds = new[]
        {
            new ProductSeed(
                "ترمووود گره ساده ضخامت ۱۹ میلیمتر عرض ۴.۲ سانت",
                "thermowood-simple-19mm-42mm",
                "پروفیل ترمووود گره ساده با ضخامت ۱۹ میلیمتر و عرض مفید ۴.۲ سانت برای روکش نما و دیوارهای داخلی. سازگار با نصب افقی و عمودی با اتصالات مخفی.",
                true,
                null,
                "product-thermowood-simple-19mm-42mm"),
            new ProductSeed(
                "ترمووود گره ساده ضخامت ۱۶ میلیمتر",
                "thermowood-simple-16mm",
                "ترمووود گره ساده با ضخامت ۱۶ میلیمتر برای پوشش سبک و سطوح داخلی. پروفیل پایدار با خطای ابعادی کم مناسب رنگ یا روغن محافظ.",
                true,
                null,
                "product-thermowood-simple-16mm"),
            new ProductSeed(
                "ترمووود گره ساده ضخامت ۱۴ میلیمتر",
                "thermowood-simple-14mm",
                "پروفیل ترمووود ۱۴ میلیمتری با بافت گره ساده برای دیوارپوش و سقف. ابعاد کم وزن اجرای سریع را ممکن می‌کند.",
                true,
                null,
                "product-thermowood-simple-14mm"),
            new ProductSeed(
                "ترمووود گره ساده ضخامت ۸ میلیمتر",
                "thermowood-simple-8mm",
                "روکش ترمووود ۸ میلیمتری برای پوشش ظریف و سطح نهایی خشک. مناسب تریم، بازشو و اصلاحات جزئی.",
                true,
                null,
                "product-thermowood-simple-8mm"),
            new ProductSeed(
                "ترمووود گره پروانه ضخامت ۱۴ میلیمتر",
                "thermowood-parvaneh-14mm",
                "ترمووود طرح پروانه ضخامت ۱۴ میلیمتر با پروفیل درزدار برای سایه‌روشن نمای خطی. سطح پایدار و آماده نصب روی زیرسازی تراز.",
                true,
                null,
                "product-thermowood-parvaneh-14mm"),
            new ProductSeed(
                "تایل ترمووود استاندارد",
                "thermowood-tile-standard",
                "تایل ترمووود استاندارد ماژولار برای کف تراس و بالکن. نصب خشکه‌چین با زیرسازی مشبک و قابلیت تعویض قطعات.",
                true,
                null,
                "product-thermowood-tile-standard"),
            new ProductSeed(
                "تایل ترمووود پازلی",
                "thermowood-tile-puzzle",
                "تایل پازلی ترمووود با قفل مکانیکی برای مونتاژ سریع. مناسب فضاهای قابل بازشدن و تعمیر دوره‌ای.",
                true,
                null,
                "product-thermowood-tile-puzzle"),
            new ProductSeed(
                "تایل چمن مصنوعی",
                "artificial-grass-tile",
                "تایل چمن مصنوعی با زیره زهکش برای محوطه و تراس. اتصال ماژولار امکان سرویس و شستشو را ساده می‌کند.",
                true,
                null,
                "product-artificial-grass-tile"),
            new ProductSeed(
                "پنل ترمووال (دیوارپوش ترمووود)",
                "thermowall-panel",
                "پنل ترمووال شیار دار برای دیوارپوش داخلی و خارجی. پشت کار متعادل برای تهویه و پیچ مخفی.",
                true,
                null,
                "product-thermowall-panel"),
            new ProductSeed(
                "تخته چوب روسی ۱۰×۵ طول ۳ متر",
                "russian-wood-10x5-3m",
                "تخته چوب روسی مقطع ۱۰×۵ به طول ۳ متر برای زیرسازی و کلاف‌بندی. چوب خشک با رطوبت کنترل‌شده.",
                true,
                null,
                "product-russian-wood-10x5-3m"),
            new ProductSeed(
                "تخته چوب روسی ۱۰×۲ طول ۳ متر",
                "russian-wood-10x2-3m",
                "تخته روسی ۱۰×۲ طول ۳ متر برای سازه‌های سبک و رابیتس. سطح صاف و مناسب رنگ یا روغن.",
                true,
                null,
                "product-russian-wood-10x2-3m"),
            new ProductSeed(
                "تخته چوب روسی ۱۰×۱.۵ طول ۳ متر",
                "russian-wood-10x15-3m",
                "تخته روسی ۱۰×۱.۵ طول ۳ متر برای روکوب و ساخت قاب. بافت یکنواخت برای نصب دقیق.",
                true,
                null,
                "product-russian-wood-10x15-3m"),
            new ProductSeed(
                "تخته چوب روسی ۱۰×۱ طول ۳ متر",
                "russian-wood-10x1-3m",
                "تخته روسی ۱۰×۱ طول ۳ متر برای پوشش لبه و کارهای ظریف. مناسب سازه‌های داخلی و سقف کاذب.",
                true,
                null,
                "product-russian-wood-10x1-3m"),
            new ProductSeed(
                "تخته چوب روسی ۱۵×۱.۵ طول ۳ متر",
                "russian-wood-15x15-3m",
                "تخته روسی ۱۵×۱.۵ طول ۳ متر برای زیرسازی و لمبه‌کشی. مقاومت مناسب برای اتصال پیچ و میخ.",
                true,
                null,
                "product-russian-wood-15x15-3m"),
            new ProductSeed(
                "تخته چوب روسی ۱۵×۲ طول ۳ متر",
                "russian-wood-15x2-3m",
                "تخته روسی ۱۵×۲ طول ۳ متر برای تیرک‌های سبک و کلاف افقی. چوب خشک آماده اجرا.",
                true,
                null,
                "product-russian-wood-15x2-3m"),
            new ProductSeed(
                "تخته چوب روسی ۵×۳ طول ۳ متر",
                "russian-wood-5x3-3m",
                "تخته روسی ۵×۳ طول ۳ متر برای شبکه‌های زیرسازی فشرده. ابعاد کم وزن نصب را سریع می‌کند.",
                true,
                null,
                "product-russian-wood-5x3-3m"),
            new ProductSeed(
                "تخته چوب روسی ۲۰×۲ طول ۳ متر",
                "russian-wood-20x2-3m",
                "تخته روسی ۲۰×۲ طول ۳ متر برای تیرک‌های طولی و لمبه زیرسازی. مناسب سازه‌های سبک و سقف.",
                true,
                null,
                "product-russian-wood-20x2-3m"),
            new ProductSeed(
                "تخته چوب روسی ۱۵×۱ طول ۳ متر",
                "russian-wood-15x1-3m",
                "تخته روسی ۱۵×۱ طول ۳ متر برای روکوب و تراز سطح. سطح صاف برای رنگ و پوشش محافظ.",
                true,
                null,
                "product-russian-wood-15x1-3m"),
            new ProductSeed(
                "تخته چوب روسی ۱۵×۵ طول ۳ متر",
                "russian-wood-15x5-3m",
                "تخته روسی ۱۵×۵ طول ۳ متر برای تیر و کلاف مقاوم. چوب خشک با گره محدود.",
                true,
                null,
                "product-russian-wood-15x5-3m"),
            new ProductSeed(
                "تخته چوب روسی ۲۰×۵ طول ۳ متر",
                "russian-wood-20x5-3m",
                "تخته روسی ۲۰×۵ طول ۳ متر برای زیرسازی با ظرفیت تحمل بیشتر. مناسب پروژه‌های سازه‌ای سبک.",
                true,
                null,
                "product-russian-wood-20x5-3m"),
            new ProductSeed(
                "چوب نیمکتی روسی ۱۰×۰.۸ طول ۳ متر",
                "russian-bench-10x08-3m",
                "چوب نیمکتی روسی ۱۰×۰.۸ طول ۳ متر با لبه نرم برای نشیمن و کلاف مبلمان. سطح سنباده شده برای پایان کاری سریع.",
                true,
                null,
                "product-russian-bench-10x08-3m"),
            new ProductSeed(
                "چوب نیمکتی روسی ۱۰×۰.۷ طول ۳ متر",
                "russian-bench-10x07-3m",
                "چوب نیمکتی روسی ۱۰×۰.۷ طول ۳ متر مناسب تکیه‌گاه و طراحی مینیمال. وزن کم و ثبات ابعادی.",
                true,
                null,
                "product-russian-bench-10x07-3m"),
            new ProductSeed(
                "نبشی روسی ۵ سانتی‌متر",
                "russian-angle-5cm",
                "نبشی روسی ۵ سانتی‌متر برای قاب‌بندی و پوشش لبه. مناسب تکمیل جزئیات چوبی و محافظت از گوشه‌ها.",
                true,
                null,
                "product-russian-angle-5cm"),
            new ProductSeed(
                "نبشی روسی ۳ سانتی‌متر",
                "russian-angle-3cm",
                "نبشی روسی ۳ سانتی‌متر سبک برای درزپوش و خط پایان. نصب آسان با پیچ یا چسب.",
                true,
                null,
                "product-russian-angle-3cm"),
            new ProductSeed(
                "فتیله چوب روسی",
                "russian-wood-strip",
                "فتیله چوب روسی برای آب‌بندی و پوشش درز بین قطعات. سطح صاف آماده رنگ یا روغن.",
                true,
                null,
                "product-russian-wood-strip"),
            new ProductSeed(
                "چوب زیرکار ۳×۲",
                "subwood-3x2",
                "چوب زیرکار ۳×۲ برای شبکه‌بندی دقیق زیر نما یا سقف. سبک و پایدار برای پشتیبانی ترمووود.",
                true,
                null,
                "product-subwood-3x2"),
            new ProductSeed(
                "چوب زیرکار ۴×۲",
                "subwood-4x2",
                "چوب زیرکار ۴×۲ برای زیرسازی کف و دیوارپوش. ابعاد رایج برای ایجاد فاصله استاندارد.",
                true,
                null,
                "product-subwood-4x2"),
            new ProductSeed(
                "چوب زیرکار ۵×۳",
                "subwood-5x3",
                "چوب زیرکار ۵×۳ برای تحمل بار بیشتر در کف یا دیوار. مناسب پیچ مستقیم و مهار لرزش.",
                true,
                null,
                "product-subwood-5x3"),
            new ProductSeed(
                "چوب زیرکار ۴×۶",
                "subwood-4x6",
                "چوب زیرکار ۴×۶ برای تیرک‌های اصلی و فریم‌های مقاوم. مقطع عمیق برای دهانه‌های بلندتر.",
                true,
                null,
                "product-subwood-4x6"),
            new ProductSeed(
                "چوب زیرکار ۵×۵",
                "subwood-5x5",
                "چوب زیرکار ۵×۵ برای شبکه زیرسازی مستحکم. مناسب پروژه‌هایی با بار متمرکز.",
                true,
                null,
                "product-subwood-5x5"),
            new ProductSeed(
                "چوب لمبه (انواع ضخامت)",
                "lambe-wood",
                "چوب لمبه با پروفیل نر و مادگی برای دیوار و سقف. آماده نصب با سطح یکنواخت و امکان رنگ‌آمیزی.",
                true,
                null,
                "product-lambe-wood")
        };

        var seedSlugs = seeds
            .Select(seed => GenerateSlug(seed.Slug))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingProducts = await dbContext.Products
            .Include(item => item.Gallery)
            .ToListAsync();

        var obsoleteProducts = existingProducts
            .Where(product => !seedSlugs.Contains(product.Slug))
            .ToList();

        if (obsoleteProducts.Count > 0)
        {
            dbContext.Products.RemoveRange(obsoleteProducts);
            await dbContext.SaveChangesAsync();
        }

        var productMap = await dbContext.Products
            .Include(item => item.Gallery)
            .Where(product => seedSlugs.Contains(product.Slug))
            .ToDictionaryAsync(product => product.Slug, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in seeds)
        {
            var normalizedSlug = GenerateSlug(seed.Slug);
            productMap.TryGetValue(normalizedSlug, out var product);

            if (product is null)
            {
                product = new Product(seed.Name, normalizedSlug, seed.Description);
                dbContext.Products.Add(product);
                productMap[normalizedSlug] = product;
            }
            else
            {
                product.UpdateDetails(seed.Name, seed.Description);
            }

            product.SetPrice(seed.Price);

            if (seed.Publish)
            {
                product.Publish();
            }

            if (mediaBySlug.TryGetValue(seed.FeaturedMediaSlug, out var featuredMedia) &&
                product.Gallery.All(item => item.Id != featuredMedia.Id))
            {
                product.AddMedia(featuredMedia);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedProjectsAsync(
        AtelierDbContext dbContext,
        IReadOnlyDictionary<string, Media> mediaBySlug)
    {
        var seeds = new[]
        {
            new ProjectSeed(
                "بازسازی نمای چوبی ویلای شمال",
                "north-villa-wood-facade",
                "اجرای نمای ترمووود با جزئیات خطی و هماهنگ با معماری مدرن ویلا.",
                "project-north-villa",
                new[] { "project-north-villa", "project-north-villa-terrace" }),
            new ProjectSeed(
                "کف سازی تراس مجتمع مسکونی",
                "residential-terrace-decking",
                "کف سازی فضای باز با ترمووود فنلاندی و طراحی مینیمال برای مقاومت بالا.",
                "project-terrace-decking",
                new[] { "project-terrace-decking", "project-terrace-decking-night" }),
            new ProjectSeed(
                "پرگولای محوطه باغ",
                "garden-pergola-project",
                "اجرای پرگولای چوبی با ترمووود مقاوم در برابر رطوبت و تابش مستقیم.",
                "project-garden-pergola",
                new[] { "project-garden-pergola", "project-garden-pergola-closeup" })
        };

        foreach (var seed in seeds)
        {
            var normalizedSlug = GenerateSlug(seed.Slug);
            var project = await dbContext.Projects.FirstOrDefaultAsync(item => item.Slug == normalizedSlug);

            if (project is not null)
            {
                continue;
            }

            mediaBySlug.TryGetValue(seed.FeaturedMediaSlug, out var featuredMedia);

            project = new Project(seed.Title, normalizedSlug, seed.Description, featuredMedia?.Id, true);

            foreach (var gallerySlug in seed.GalleryMediaSlugs)
            {
                if (mediaBySlug.TryGetValue(gallerySlug, out var galleryMedia))
                {
                    project.AddMedia(galleryMedia);
                }
            }

            dbContext.Projects.Add(project);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedLeadsAsync(AtelierDbContext dbContext)
    {
        var exists = await dbContext.Leads.AnyAsync();
        if (exists)
        {
            return;
        }

        var lead = new Lead(
            "کاربر نمونه",
            "sample@woodzilla.ir",
            "برای دریافت مشاوره درباره انتخاب ترمووود مناسب پروژه تماس بگیرید.");

        dbContext.Leads.Add(lead);
        await dbContext.SaveChangesAsync();
    }

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return $"slug-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var normalized = input.Trim().ToLowerInvariant();
        var chars = normalized.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        slug = string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(slug)
            ? $"slug-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : slug;
    }

    private sealed record CategorySeed(
        string Name,
        string Slug,
        string? ParentSlug);

    private sealed record MediaSeed(
        string Slug,
        string Url,
        string Title,
        string AltText,
        string FileName,
        string Purpose,
        string SourceUrl,
        bool IsNotDownloaded);

    private sealed record PageSeed(
        string Title,
        string Slug,
        string Content,
        string MetaTitle,
        string MetaDescription,
        string FeaturedMediaSlug);

    private sealed record BlogPostSeed(
        string Title,
        string Slug,
        string Excerpt,
        string Content,
        string MetaTitle,
        string MetaDescription,
        string FeaturedMediaSlug);

    private sealed record ProjectSeed(
        string Title,
        string Slug,
        string Description,
        string FeaturedMediaSlug,
        IReadOnlyList<string> GalleryMediaSlugs);

    private sealed record ProductSeed(
        string Name,
        string Slug,
        string Description,
        bool Publish,
        decimal? Price,
        string FeaturedMediaSlug);

    private static async Task SeedCategoryMediaAsync(
        AtelierDbContext dbContext,
        IReadOnlyDictionary<string, Media> mediaBySlug)
    {
        var seeds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["thermowood"] = "category-thermowood-main",
            ["thermowood-tiles"] = "category-thermowood-tiles",
            ["artificial-grass-tiles"] = "category-artificial-grass-tiles",
            ["thermowall"] = "category-thermowall",
            ["russian-wood"] = "category-russian-wood",
            ["lambe"] = "category-lambe"
        };

        var categories = await dbContext.Categories.ToListAsync();

        foreach (var category in categories)
        {
            if (!seeds.TryGetValue(category.Slug, out var mediaSlug))
            {
                continue;
            }

            if (!mediaBySlug.TryGetValue(mediaSlug, out var media))
            {
                continue;
            }

            category.UpdateMedia(media.Id);
        }

        await dbContext.SaveChangesAsync();
    }
}
