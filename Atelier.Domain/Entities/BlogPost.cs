public class BlogPost : Entity
{
    public string Title { get; private set; }
    public string Slug { get; private set; }
    public string Excerpt { get; private set; }
    public string Content { get; private set; }
    public int? FeaturedMediaId { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }

    protected BlogPost()
    {
        Title = "N/A";
        Slug = "n-a";
        Excerpt = "";
        Content = "";
    }

    public BlogPost(
        string title,
        string slug,
        string excerpt,
        string content,
        int? featuredMediaId,
        string? metaTitle,
        string? metaDescription)
    {
        Title = title;
        Slug = slug;
        Excerpt = excerpt;
        Content = content;
        FeaturedMediaId = featuredMediaId;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
    }

    public void UpdateDetails(
        string title,
        string slug,
        string excerpt,
        string content,
        int? featuredMediaId,
        string? metaTitle,
        string? metaDescription)
    {
        Title = title;
        Slug = slug;
        Excerpt = excerpt;
        Content = content;
        FeaturedMediaId = featuredMediaId;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MarkUpdated();
    }

    public void Publish()
    {
        PublishedAt = DateTimeOffset.UtcNow;
        MarkUpdated();
    }

    public void Unpublish()
    {
        PublishedAt = null;
        MarkUpdated();
    }
}
