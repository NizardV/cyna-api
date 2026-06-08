namespace Application.Dto.Home;

/// <summary>
/// DTO regroupant toutes les données nécessaires à l'affichage de la page d'accueil en une seule requête.
/// </summary>
public class HomePageDto
{
    /// <summary>
    /// Liste des slides actifs pour le carrousel principal.
    /// </summary>
    public IEnumerable<CarouselSlideDto> CarouselSlides { get; set; } = new List<CarouselSlideDto>();

    /// <summary>
    /// Le texte fixe de présentation (Mission) de l'entreprise affiché sous le carrousel.
    /// </summary>
    public string? MissionText { get; set; }

    /// <summary>
    /// Liste des catégories de produits à afficher sur la page d'accueil.
    /// </summary>
    public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
}