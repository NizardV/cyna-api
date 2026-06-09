namespace Api.Controllers;

using Application.Interfaces;

using Domain.Dto.User;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.ResetTokenAsync(request);
        return result == null ? BadRequest(new { message = "Impossible de générer un nouveau token." }) : Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        var success = await _authService.LogoutAsync(request.RefreshToken);
        return success ? Ok(new { message = "Déconnexion réussie." }) : BadRequest(new { message = "Token invalide." });
    }

}