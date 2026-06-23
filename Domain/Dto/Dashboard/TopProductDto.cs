namespace Domain.Dto.Dashboard;

/// <summary>
/// Critère de tri pour le classement des produits les plus performants.
/// </summary>
public enum TopProductSortBy
{
    /// <summary>Trié par chiffre d'affaires généré (somme des lignes de commande payées).</summary>
    Revenue,

    /// <summary>Trié par nombre de commandes contenant ce produit.</summary>
    Orders,
}

/// <summary>
/// Produit classé dans le top des meilleures ventes du dashboard admin.
/// </summary>
/// <remarks>
/// NOTE TEMPORAIRE : voir <see cref="RevenueStatsDto"/> — le chiffre d'affaires par produit
/// dépend des commandes payées, dont le flux de paiement est en cours de développement.
/// Cette route accepte <c>?mock=true</c> pour des données factices générées par Bogus.
/// </remarks>
public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Revenue { get; set; }
    public int OrdersCount { get; set; }
}