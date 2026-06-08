using Application.Dtos;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<LoginResultDto?> LoginAsync(LoginRequestDto request);
    Task<bool> RegisterAsync(RegisterRequestDto request);
    Task<LoginResultDto?> ResetTokenAsync(ResetTokenRequestDto request);
}