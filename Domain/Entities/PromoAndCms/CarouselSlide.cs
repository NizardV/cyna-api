namespace Domain.Entities.PromoAndCms;

using System.ComponentModel.DataAnnotations;

using Infrastructure.Entities.PromoAndCms;

public class CarouselSlide
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string ImageUrl { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<CarouselSlideTranslation> Translations { get; set; } = [];
}