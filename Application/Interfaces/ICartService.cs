namespace Application.Interfaces.Services;

using Domain.Dto.Cart;

/// <summary>
/// Interface du service de gestion du panier d'achat.
/// Orchestre l'ajout et la mise à jour des articles, le calcul des prix par palier et le récapitulatif du panier.
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Ajoute un article au panier ou met à jour les quantités si le plan tarifaire est déjà présent.
    /// Calcule le prix unitaire en fonction des paliers tarifaires du plan et renvoie le récapitulatif complet.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur authentifié.</param>
    /// <param name="dto">Le plan tarifaire sélectionné et les quantités (utilisateurs et/ou appareils).</param>
    /// <returns>L'état mis à jour du panier avec les montants HT, TVA et TTC.</returns>
    /// <exception cref="ArgumentException">Si les quantités utilisateurs et appareils sont toutes deux à zéro.</exception>
    /// <exception cref="KeyNotFoundException">Si le plan tarifaire est introuvable.</exception>
    Task<CartResultDto> AddOrUpdateCartItemAsync(int userId, AddCartItemRequestDto dto);
}
