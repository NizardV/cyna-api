using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class ResetTokenRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}