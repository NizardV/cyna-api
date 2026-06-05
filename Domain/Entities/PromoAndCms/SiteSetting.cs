namespace Domain.Entities.PromoAndCms;

using System.ComponentModel.DataAnnotations;

using Infrastructure.Entities.PromoAndCms;

public class SiteSetting
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string SettingKey { get; set; } = string.Empty;

    public ICollection<SiteSettingTranslation> Translations { get; set; } = [];
}