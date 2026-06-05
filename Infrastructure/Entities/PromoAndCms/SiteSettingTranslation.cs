namespace Infrastructure.Entities.PromoAndCms;

using System.ComponentModel.DataAnnotations;

public class SiteSettingTranslation
{
    public int Id { get; set; }
    public int SettingId { get; set; }
    public LocaleLang Locale { get; set; }

    [Required]
    public string SettingValue { get; set; } = string.Empty;

    public SiteSetting Setting { get; set; } = null!;
}