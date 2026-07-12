using System;

public class Media : Entity
{
    public string Slug { get; private set; }
    public string Url { get; private set; }
    public int? StorageId { get; private set; }
    public string? ContentType { get; private set; }
    public long? FileSize { get; private set; }
    public string? Title { get; private set; }
    public string? AltText { get; private set; }
    public string? FileName { get; private set; }
    public string? Purpose { get; private set; }
    public string? SourceUrl { get; private set; }
    public bool IsNotDownloaded { get; private set; }

    protected Media()
    {
        Slug = "N/A";
        Url = "N/A";
    }
    public Media(
        string slug,
        string url,
        int? storageId,
        string? contentType,
        long? fileSize,
        string? title = null,
        string? altText = null,
        string? fileName = null,
        string? purpose = null,
        string? sourceUrl = null,
        bool isNotDownloaded = false)
    {
        EnsureAltTextForImages(contentType, altText);
        Slug = slug;
        Url = url;
        StorageId = storageId;
        ContentType = contentType;
        FileSize = fileSize;
        Title = title;
        AltText = altText;
        FileName = fileName;
        Purpose = purpose;
        SourceUrl = sourceUrl;
        IsNotDownloaded = isNotDownloaded;
    }

    public void UpdateMetadata(string? title, string? altText)
    {
        EnsureAltTextForImages(ContentType, altText);
        Title = title;
        AltText = altText;
        MarkUpdated();
    }

    public void UpdateUrl(string url)
    {
        Url = url;
        MarkUpdated();
    }

    public void UpdateDownloadStatus(bool isNotDownloaded)
    {
        IsNotDownloaded = isNotDownloaded;
        MarkUpdated();
    }

    public void AttachStoredContent(int storageId, string contentType, long fileSize, string url)
    {
        if (storageId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(storageId));
        }

        if (string.IsNullOrWhiteSpace(contentType) ||
            !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Stored product media must be an image.", nameof(contentType));
        }

        StorageId = storageId;
        ContentType = contentType;
        FileSize = fileSize;
        Url = url;
        IsNotDownloaded = false;
        MarkUpdated();
    }

    public void UpdateDetails(
        string url,
        string? title,
        string? altText,
        string? fileName,
        string? purpose,
        string? sourceUrl,
        bool isNotDownloaded)
    {
        EnsureAltTextForImages(ContentType, altText);
        Url = url;
        Title = title;
        AltText = altText;
        FileName = fileName;
        Purpose = purpose;
        SourceUrl = sourceUrl;
        IsNotDownloaded = isNotDownloaded;
        MarkUpdated();
    }

    private static void EnsureAltTextForImages(string? contentType, string? altText)
    {
        if (IsImageContentType(contentType) && string.IsNullOrWhiteSpace(altText))
        {
            throw new ArgumentException("Alt text is required for image media.", nameof(altText));
        }
    }

    private static bool IsImageContentType(string? contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType)
            && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }
}
