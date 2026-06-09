using Domain.Dto.User;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto?> LoginAsync(LoginRequestDto request);
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResultDto?> ResetTokenAsync(RefreshTokenRequestDto request);
    Task<bool> LogoutAsync(string refreshToken);
}