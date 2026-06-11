using Domain.Entities.PromoAndCms;

using Tools;

namespace Infrastructure.Interfaces;

/// <summary>
/// Gère l'accès aux données des slides du carrousel promotionnel.
/// </summary>
public interface ICarouselRepository
{
    /// <summary>
    /// Récupère tous les slides actifs, triés par leur ordre d'affichage, en incluant uniquement la traduction correspondant à la langue spécifiée.
    /// </summary>
    /// <param name="locale">La langue souhaitée (ex: Français, Anglais).</param>
    /// <returns>Une collection de <see cref="CarouselSlide"/> contenant les traductions filtrées.</returns>
    Task<IEnumerable<CarouselSlide>> GetActiveSlidesAsync(LocaleLang locale);
}