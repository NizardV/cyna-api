namespace Domain.Entities.PromoAndCms;

using Tools;

public class CarouselSlideTranslation
{
    public int Id { get; set; }
    public int SlideId { get; set; }
    public LocaleLang Locale { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? ButtonText { get; set; }

    public CarouselSlide Slide { get; set; } = null!;
}