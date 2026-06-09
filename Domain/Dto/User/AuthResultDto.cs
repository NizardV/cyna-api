namespace Domain.Dto.User;

public class AuthResultDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? ErrorMessage { get; set; }
}
