namespace Atelier.Web.Areas.Admin.ViewModels;

public class MediaManagerPickerViewModel
{
    public IReadOnlyList<MediaManagerItemViewModel> Items { get; set; } = Array.Empty<MediaManagerItemViewModel>();
    public string MaxUploadSizeDisplay { get; set; } = "";
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
    public int NextPage => Page + 1;
}

public class MediaManagerItemViewModel
{
    public int Id { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string SizedUrl { get; set; } = string.Empty;
    public string SrcSet { get; set; } = string.Empty;
    public bool HasAltText => !string.IsNullOrWhiteSpace(AltText);
}
