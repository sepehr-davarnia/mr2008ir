
public class Project : Entity
{
    private readonly List<Media> _gallery = new();

    public string Title { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public int? FeaturedMediaId { get; private set; }
    public bool IsPublished { get; private set; }
    public IReadOnlyCollection<Media> Gallery => _gallery.AsReadOnly();

    protected Project()
    {
        Slug = "N/A";
        Title = "N/A";
    }

    public Project(string title, string slug, string? description = null, int? featuredMediaId = null, bool isPublished = false)
    {
        Title = title;
        Slug = slug;
        Description = description;
        FeaturedMediaId = featuredMediaId;
        IsPublished = isPublished;
    }

    public void UpdateDetails(string title, string? description, int? featuredMediaId, bool isPublished)
    {
        Title = title;
        Description = description;
        FeaturedMediaId = featuredMediaId;
        IsPublished = isPublished;
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

    public void Publish()
    {
        IsPublished = true;
        MarkUpdated();
    }

    public void Unpublish()
    {
        IsPublished = false;
        MarkUpdated();
    }
}
