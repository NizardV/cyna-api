namespace Domain.Dto.Catalog;

/// <summary>
/// Résultat paginé d'une recherche dans le catalogue.
/// </summary>
public class CatalogPageDto
{
    /// <summary>Nombre total de produits correspondant aux filtres.</summary>
    public int Total { get; set; }

    /// <summary>Numéro de page courant (base 1).</summary>
    public int Page { get; set; }

    /// <summary>Nombre d'éléments par page.</summary>
    public int PageSize { get; set; }

    /// <summary>Nombre total de pages.</summary>
    public int TotalPages { get; set; }

    /// <summary>Produits de la page courante.</summary>
    public IEnumerable<ProductDto> Items { get; set; } = [];
}