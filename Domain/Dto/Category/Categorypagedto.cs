namespace Domain.Dto.Category;

/// <summary>
/// Résultat paginé de la liste des catégories.
/// </summary>
public class CategoryPageDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<CategoryDto> Items { get; set; } = [];
}

