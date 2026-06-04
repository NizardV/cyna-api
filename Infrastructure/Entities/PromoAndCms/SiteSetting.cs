namespace Infrastructure.Entities.PromoAndCms;

public class SiteSetting
{
    public int Id { get; set; }
    public string SettingKey { get; set; } = string.Empty; // ex: homepage_mission_text

    public ICollection<SiteSettingTranslation> Translations { get; set; } = [];
}