namespace Api.Controllers;

using System.Security.Claims;

using Application.Interfaces;

using Domain.Dto.User;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Tools;

/// <summary>
/// Contrôleur d'authentification.
/// Gère la connexion, l'inscription, le renouvellement de token et la déconnexion.
/// Les jetons JWT sont transmis via des cookies HttpOnly (jamais dans le corps de la réponse).
/// </summary>
[ApiController]
[Route("auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="AuthController"/>.
    /// </summary>
    /// <param name="authService">Le service d'authentification.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authentifie un utilisateur avec son email et son mot de passe.
    /// En cas de succès, injecte les tokens JWT dans des cookies HttpOnly.
    /// </summary>
    /// <param name="request">L'email et le mot de passe de l'utilisateur.</param>
    /// <returns>Un message de confirmation (les tokens sont dans les cookies).</returns>
    /// <response code="200">Connexion réussie.</response>
    /// <response code="401">Identifiants invalides.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        AppendAuthCookie("cyna_token", result.Token!, minutes: 15);
        AppendAuthCookie("cyna_refresh_token", result.RefreshToken!, minutes: 1440);

        return Ok(new { message = "Connexion réussie." });
    }

    /// <summary>
    /// Crée un nouveau compte utilisateur.
    /// </summary>
    /// <param name="request">Les informations d'inscription (prénom, nom, email, mot de passe).</param>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Inscription réussie.</response>
    /// <response code="400">Email déjà utilisé ou données invalides.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.Success ? Ok(new { message = "Inscription réussie." }) : BadRequest(new { message = result.ErrorMessage });
    }

    /// <summary>
    /// Renouvelle l'access token à partir du refresh token présent dans les cookies.
    /// </summary>
    /// <returns>Un message de confirmation (les nouveaux tokens sont dans les cookies).</returns>
    /// <response code="200">Tokens rafraîchis.</response>
    /// <response code="400">Refresh token absent, invalide ou expiré.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh()
    {
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

        AppendAuthCookie("cyna_token", result.Token!, minutes: 15);
        AppendAuthCookie("cyna_refresh_token", result.RefreshToken!, minutes: 1440);

        return Ok(new { message = "Tokens rafraîchis." });
    }

    /// <summary>
    /// Déconnecte l'utilisateur en invalidant le refresh token en base et en supprimant les cookies.
    /// </summary>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Déconnexion réussie.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var currentRefreshToken = Request.Cookies["cyna_refresh_token"];

        if (!string.IsNullOrEmpty(currentRefreshToken))
        {
            await _authService.LogoutAsync(currentRefreshToken);
        }

        Response.Cookies.Delete("cyna_token", GetCookieOptions(expired: true));
        Response.Cookies.Delete("cyna_refresh_token", GetCookieOptions(expired: true));

        return Ok(new { message = "Déconnexion réussie." });
    }

    /// <summary>
    /// Retourne les informations de l'utilisateur actuellement connecté à partir de son token JWT.
    /// </summary>
    /// <returns>Les claims de l'utilisateur (id, prénom, nom, email, rôle).</returns>
    /// <response code="200">Utilisateur authentifié.</response>
    /// <response code="401">Token absent ou invalide.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        var firstNameClaim = User.FindFirst("firstName")?.Value;
        var lastNameClaim = User.FindFirst("lastName")?.Value;
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        var roleClaim  = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        return Ok(new
        {
            Id = userIdClaim,
            FirstName = firstNameClaim,
            LastName = lastNameClaim,
            Email = emailClaim,
            Role = roleClaim
        });
    }

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
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = expired ? DateTime.UtcNow.AddDays(-1) : null
        };
    }

}