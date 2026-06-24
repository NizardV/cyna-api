namespace Application.Dto.Home;

/// <summary>
/// DTO représentant la carte résumé d'un produit (ex: Top Products).
/// </summary>
public class ProductSummaryDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? StartingPrice { get; set; }
    public string? Status { get; set; }
}