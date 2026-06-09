namespace Api.Controllers;

using Application.Interfaces;
using Domain.Dto.User;
using Microsoft.AspNetCore.Http;
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

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        // 1. On injecte l'Access Token dans un cookie HttpOnly
        AppendAuthCookie("cyna_token", result.Token!, minutes: 15);

        // 2. On injecte le Refresh Token dans un autre cookie HttpOnly
        AppendAuthCookie("cyna_refresh_token", result.RefreshToken!, minutes: 1440); // 24h

        // 3. On ne renvoie RIEN dans le body JSON (les jetons sont masqués dans les en-têtes HTTP !)
        return Ok(new { message = "Connexion réussie." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.Success ? Ok(new { message = "Inscription réussie." }) : BadRequest(new { message = result.ErrorMessage });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        // 1. On va chercher le refresh token DIRECTEMENT dans les cookies de la requête
        var currentRefreshToken = Request.Cookies["cyna_refresh_token"];

        if (string.IsNullOrEmpty(currentRefreshToken))
        {
            return BadRequest(new { message = "Aucun refresh token fourni." });
        }

        var requestDto = new RefreshTokenRequestDto { RefreshToken = currentRefreshToken };
        var result = await _authService.ResetTokenAsync(requestDto);

        if (result == null)
        {
            return BadRequest(new { message = "Impossible de générer un nouveau token." });
        }

        // 2. On écrase les anciens cookies avec les nouveaux jetons
        AppendAuthCookie("cyna_token", result.Token!, minutes: 15);
        AppendAuthCookie("cyna_refresh_token", result.RefreshToken!, minutes: 1440);

        return Ok(new { message = "Tokens rafraîchis." });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // 1. On récupère le token pour l'invalider en BDD
        var currentRefreshToken = Request.Cookies["cyna_refresh_token"];

        if (!string.IsNullOrEmpty(currentRefreshToken))
        {
            await _authService.LogoutAsync(currentRefreshToken);
        }

        // 2. On SUPPRIME les cookies du navigateur en les forçant à expirer immédiatement
        Response.Cookies.Delete("cyna_token", GetCookieOptions(expired: true));
        Response.Cookies.Delete("cyna_refresh_token", GetCookieOptions(expired: true));

        return Ok(new { message = "Déconnexion réussie." });
    }

    // ---------------------------------------------------------------------------
    // HELPERS PRIVÉS POUR SÉCURISER LES COOKIES
    // ---------------------------------------------------------------------------

    private void AppendAuthCookie(string key, string value, int minutes)
    {
        var options = GetCookieOptions();
        options.Expires = DateTime.UtcNow.AddMinutes(minutes);
        Response.Cookies.Append(key, value, options);
    }

    private CookieOptions GetCookieOptions(bool expired = false)
    {
        return new CookieOptions
        {
            HttpOnly = true,             // EMPÊCHE LE VOL DE TOKEN PAR FAILLE XSS (JavaScript ne peut pas le lire)
            Secure = true,               // Uniquement transmis en HTTPS (indispensable pour ton port local 7169)
            SameSite = SameSiteMode.None, // Requis en cross-origin local (React :5173 vs API :7169). Devra être mis à SameSiteMode.Strict en prod s'ils partagent le même nom de domaine.
            Path = "/",
            Expires = expired ? DateTime.UtcNow.AddDays(-1) : null
        };
    }

}