using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class UpdateGameDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Platform { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Genre { get; set; } = string.Empty;

    public DateTime? ReleaseDate { get; set; }
}
