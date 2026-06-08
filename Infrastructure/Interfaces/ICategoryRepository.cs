using Domain.Entities.Catalogue;

using Tools;

namespace Infrastructure.Interfaces;

/// <summary>
/// Contrat pour la gestion des catégories du catalogue.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Récupère toutes les catégories triées par ordre d'affichage avec leur traduction.
    /// </summary>
    Task<IEnumerable<Category>> GetCategoriesAsync(LocaleLang locale);
}