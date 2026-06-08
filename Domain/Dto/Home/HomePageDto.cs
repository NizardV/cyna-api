namespace Application.Dto.Home;

/// <summary>
/// DTO regroupant toutes les données nécessaires à l'affichage de la page d'accueil en une seule requête.
/// </summary>
public class HomePageDto
{
    public IEnumerable<CarouselSlideDto> CarouselSlides { get; set; } = new List<CarouselSlideDto>();
}