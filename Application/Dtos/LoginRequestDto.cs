using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress] // Optionnel, mais idéal pour valider le format côté API
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}