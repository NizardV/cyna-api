namespace Infrastructure.Entities.PromoAndCms;

public class SiteSettingTranslation
{
    public int Id { get; set; }
    public int SettingId { get; set; }
    public LocaleLang Locale { get; set; }
    public string SettingValue { get; set; } = string.Empty;

    public SiteSetting Setting { get; set; } = null!;
}