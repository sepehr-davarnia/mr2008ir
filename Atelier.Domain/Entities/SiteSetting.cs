public class SiteSetting : Entity
{
    public string Key { get; private set; }
    public string Value { get; private set; }
    public int MaxUploadSizeKb { get; private set; }
    public string? SiteName { get; private set; }
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? Mobile { get; private set; }
    public string? WhatsApp { get; private set; }
    public string? Instagram { get; private set; }
    public string? Telegram { get; private set; }
    public string? Email { get; private set; }
    public int? LogoMediaId { get; private set; }
    public int? FaviconMediaId { get; private set; }
    public int? HomeHeroMediaId { get; private set; }
    public int? HomeSecondaryMediaId { get; private set; }
    public int? DefaultCategoryMediaId { get; private set; }

    protected SiteSetting()
    {
        Key = "N/A";
        Value = "N/A";
        MaxUploadSizeKb = 5120;
    }

    public SiteSetting(string key, string value)
    {
        Key = key;
        Value = value;
        MaxUploadSizeKb = 5120;
    }

    public SiteSetting(int maxUploadSizeKb)
    {
        Key = "General";
        Value = string.Empty;
        MaxUploadSizeKb = maxUploadSizeKb;
    }

    public void UpdateValue(string value)
    {
        Value = value;
        MarkUpdated();
    }

    public void UpdateMaxUploadSizeKb(int maxUploadSizeKb)
    {
        MaxUploadSizeKb = maxUploadSizeKb;
        MarkUpdated();
    }

    public void UpdateBranding(int? logoMediaId, int? faviconMediaId)
    {
        LogoMediaId = logoMediaId;
        FaviconMediaId = faviconMediaId;
        MarkUpdated();
    }

    public void UpdateVisualMedia(int? homeHeroMediaId, int? homeSecondaryMediaId, int? defaultCategoryMediaId)
    {
        HomeHeroMediaId = homeHeroMediaId;
        HomeSecondaryMediaId = homeSecondaryMediaId;
        DefaultCategoryMediaId = defaultCategoryMediaId;
        MarkUpdated();
    }

    public void UpdateContactInfo(
        string? siteName,
        string? address,
        string? phone,
        string? mobile,
        string? whatsApp,
        string? instagram,
        string? telegram,
        string? email)
    {
        SiteName = siteName;
        Address = address;
        Phone = phone;
        Mobile = mobile;
        WhatsApp = whatsApp;
        Instagram = instagram;
        Telegram = telegram;
        Email = email;
        MarkUpdated();
    }
}
