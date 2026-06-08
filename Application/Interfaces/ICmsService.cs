using Application.Dto.Home;

using Tools;

namespace Application.Interfaces;

/// <summary>
/// Service de gestion du contenu (CMS) pour alimenter les pages vitrines de l'application.
/// </summary>
public interface ICmsService
{
    /// <summary>
    /// Récupère et formate les données du carrousel pour la page d'accueil.
    /// </summary>
    /// <param name="locale">La langue dans laquelle les textes doivent être retournés.</param>
    /// <returns>Une liste de <see cref="CarouselSlideDto"/> prête à être consommée par le front-end.</returns>
    Task<IEnumerable<CarouselSlideDto>> GetHomeCarouselAsync(LocaleLang locale);
}