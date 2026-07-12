using Atelier.Domain.Enums;

public class Product : Entity
{
    private readonly List<Media> _gallery = new();

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public ProductStatus Status { get; private set; }
    public decimal? Price { get; private set; }
    public PriceType PriceType { get; private set; }
    public IReadOnlyCollection<Media> Gallery => _gallery.AsReadOnly();

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
}
