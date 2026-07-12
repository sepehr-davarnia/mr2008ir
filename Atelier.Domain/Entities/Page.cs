using Atelier.Domain.Enums;
using System;

public class Page : Entity
{
    public string Title { get; private set; }
    public string Slug { get; private set; }
    public string? Content { get; private set; }
    public int? FeaturedMediaId { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public PageStatus Status { get; private set; }
    public bool IsPublished => Status == PageStatus.Published;

    protected Page()
    {
        Slug = "N/A";
        Title = "N/A";
    }

    public Page(string title, string slug, string? content = null, int? featuredMediaId = null, string? metaTitle = null, string? metaDescription = null)
    {
        Title = title;
        Slug = slug;
        Content = content;
        FeaturedMediaId = featuredMediaId;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        Status = PageStatus.Draft;
    }

    public void UpdateContent(string title, string? content, int? featuredMediaId, string? metaTitle, string? metaDescription)
    {
        Title = title;
        Content = content;
        FeaturedMediaId = featuredMediaId;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MarkUpdated();
    }

    public void Publish()
    {
        Status = PageStatus.Published;
        MarkUpdated();
    }

    public void MarkDraft()
    {
        Status = PageStatus.Draft;
        MarkUpdated();
    }

    public void Unpublish()
    {
        MarkDraft();
    }
}
