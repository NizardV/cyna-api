using Application.Dtos;
using Application.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

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

        if (!result.Success)
        {
            // Option A : Tu renvoies quand même un 401 pour être Standard HTTP rest
            return Unauthorized(result);

            // Option B : Si ton Front préfère recevoir du 200 OK même si ça a échoué (vu la structure du DTO) :
            // return Ok(result);
        }

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var success = await _authService.RegisterAsync(request);
        if (!success)
        {
            return BadRequest(new { message = "Cet email est déjà utilisé." });
        }
        return Ok(new { message = "Compte créé avec succès. Un e-mail de confirmation va vous être envoyé." });
    }

    [HttpPost("reset-token")]
    public async Task<IActionResult> ResetToken([FromBody] ResetTokenRequestDto request)
    {
        var result = await _authService.ResetTokenAsync(request);
        if (result == null)
        {
            return BadRequest(new { message = "Impossible de générer un nouveau token." });
        }
        return Ok(result);
    }
}