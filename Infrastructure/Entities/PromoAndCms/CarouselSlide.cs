namespace Infrastructure.Entities.PromoAndCms;

public class CarouselSlide
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<CarouselSlideTranslation> Translations { get; set; } = [];
}