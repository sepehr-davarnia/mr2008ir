using Atelier.Domain.Enums;

public class Product : Entity
{
    private readonly List<Media> _gallery = new();
    private readonly List<Category> _categories = new();
    private readonly List<ProductCompatibility> _compatibilities = new();

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public string? Brand { get; private set; }
    public string? Manufacturer { get; private set; }
    public string? OemPartNumber { get; private set; }
    public string? TechnicalPartNumber { get; private set; }
    public string? AlternatePartNumbers { get; private set; }
    public ProductStatus Status { get; private set; }
    public decimal? Price { get; private set; }
    public PriceType PriceType { get; private set; }
    public IReadOnlyCollection<Media> Gallery => _gallery.AsReadOnly();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();
    public IReadOnlyCollection<ProductCompatibility> Compatibilities => _compatibilities.AsReadOnly();

    protected Product()
    {
        Slug = "N/A";
        Name = "N/A";
        PriceType = PriceType.Contact;
    }

    public Product(string name, string slug, string? description = null)
    {
        Name = name;
        Slug = slug;
        Description = description;
        Status = ProductStatus.Draft;
        PriceType = PriceType.Contact;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
        MarkUpdated();
    }

    public void UpdateCommerceDetails(string? brand, string? manufacturer, string? oemPartNumber,
        string? technicalPartNumber, string? alternatePartNumbers)
    {
        Brand = Normalize(brand);
        Manufacturer = Normalize(manufacturer);
        OemPartNumber = Normalize(oemPartNumber);
        TechnicalPartNumber = Normalize(technicalPartNumber);
        AlternatePartNumbers = Normalize(alternatePartNumbers);
        MarkUpdated();
    }

    public void SetCategories(IEnumerable<Category> categories)
    {
        _categories.Clear();
        foreach (var category in categories.DistinctBy(item => item.Id)) _categories.Add(category);
        MarkUpdated();
    }

    public void SetCompatibilities(IEnumerable<ProductCompatibility> compatibilities)
    {
        _compatibilities.Clear();
        _compatibilities.AddRange(compatibilities);
        MarkUpdated();
    }

    public void AddMedia(Media media)
    {
        _gallery.Add(media);
        MarkUpdated();
    }

    public void RemoveMedia(Media media)
    {
        _gallery.Remove(media);
        MarkUpdated();
    }

    public void SetPrice(decimal? price)
    {
        if (price is not null && price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        Price = price > 0 ? price : null;
        PriceType = Price.HasValue ? PriceType.Fixed : PriceType.Contact;
        MarkUpdated();
    }

    public void Publish()
    {
        Status = ProductStatus.Published;
        MarkUpdated();
    }

    public void Archive()
    {
        Status = ProductStatus.Archived;
        MarkUpdated();
    }

    public void MarkDraft()
    {
        Status = ProductStatus.Draft;
        MarkUpdated();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
