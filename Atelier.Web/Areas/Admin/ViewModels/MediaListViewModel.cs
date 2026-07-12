using System;
using System.Collections.Generic;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class MediaListViewModel
{
    public IReadOnlyList<MediaListItemViewModel> Items { get; set; } = Array.Empty<MediaListItemViewModel>();
}

public class MediaListItemViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? AltText { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public int? ProductId { get; set; }
}
