namespace Application.Dto.Home;

/// <summary>
/// DTO représentant une catégorie aplatie pour l'affichage Front-End.
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}