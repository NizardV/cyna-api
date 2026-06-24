namespace Api.Controllers;

using System.Security.Claims;

using Application.Interfaces;
using Application.Services;

using Domain.Dto.User;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Tools;

/// <summary>
/// Contrôleur d'authentification.
/// Gère connexion, inscription, renouvellement de token, déconnexion,
/// réinitialisation de mot de passe par OTP, confirmation d'email et 2FA admin.
/// Les jetons JWT sont transmis via des cookies HttpOnly.
/// </summary>
[ApiController]
[Route("auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AuthService  _authServiceConcrete; // For internal helpers not in the interface

    public AuthController(IAuthService authService, AuthService authServiceConcrete)
    {
        _authService         = authService;
        _authServiceConcrete = authServiceConcrete;
    }

    // ── POST /auth/login ──────────────────────────────────────────────────────

    /// <summary>Authentifie un utilisateur (hors admin avec 2FA actif).</summary>
    /// <response code="200">Connexion réussie.</response>
    /// <response code="401">Identifiants invalides ou compte désactivé.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(new { message = result.ErrorMessage });

        AppendAuthCookies(result);
        return Ok(new { message = "Connexion réussie." });
    }

    // ── POST /auth/register ───────────────────────────────────────────────────

    /// <summary>
    /// Crée un nouveau compte utilisateur et envoie un OTP de vérification email.
    /// </summary>
    /// <response code="200">Inscription réussie. Un code de vérification a été envoyé par email.</response>
    /// <response code="400">Email déjà utilisé ou données invalides.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.Success
            ? Ok(new { message = "Inscription réussie. Veuillez vérifier votre email." })
            : BadRequest(new { message = result.ErrorMessage });
    }

    // ── POST /auth/refresh ────────────────────────────────────────────────────

    /// <summary>Renouvelle l'access token via le refresh token cookie.</summary>
    /// <response code="200">Tokens rafraîchis.</response>
    /// <response code="400">Refresh token absent, invalide, expiré ou compte désactivé.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh()
    {
        var currentRefreshToken = Request.Cookies["cyna_refresh_token"];
        if (string.IsNullOrEmpty(currentRefreshToken))
            return BadRequest(new { message = "Aucun refresh token fourni." });

        var result = await _authService.ResetTokenAsync(new RefreshTokenRequestDto { RefreshToken = currentRefreshToken });
        if (result is null)
            return BadRequest(new { message = "Impossible de générer un nouveau token." });

        AppendAuthCookies(result);
        return Ok(new { message = "Tokens rafraîchis." });
    }

    // ── POST /auth/logout ─────────────────────────────────────────────────────

    /// <summary>Déconnecte l'utilisateur et supprime les cookies.</summary>
    /// <response code="200">Déconnexion réussie.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var currentRefreshToken = Request.Cookies["cyna_refresh_token"];
        if (!string.IsNullOrEmpty(currentRefreshToken))
            await _authService.LogoutAsync(currentRefreshToken);

        Response.Cookies.Delete("cyna_token",         GetCookieOptions(expired: true));
        Response.Cookies.Delete("cyna_refresh_token", GetCookieOptions(expired: true));

        return Ok(new { message = "Déconnexion réussie." });
    }

    // ── GET /auth/me ──────────────────────────────────────────────────────────

    /// <summary>Retourne les claims de l'utilisateur connecté.</summary>
    /// <response code="200">Utilisateur authentifié.</response>
    /// <response code="401">Token absent ou invalide.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userIdClaim    = User.FindFirst("id")?.Value;
        var firstNameClaim = User.FindFirst("firstName")?.Value;
        var lastNameClaim  = User.FindFirst("lastName")?.Value;
        var emailClaim     = User.FindFirst("email")?.Value;
        var roleClaim      = User.FindFirst("role")?.Value;

        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        return Ok(new
        {
            Id        = userIdClaim,
            FirstName = firstNameClaim,
            LastName  = lastNameClaim,
            Email     = emailClaim,
            Role      = roleClaim
        });
    }

    // ── POST /auth/forgot-password ────────────────────────────────────────────

    /// <summary>
    /// Envoie un code OTP de réinitialisation de mot de passe à l'email fourni.
    /// Répond toujours 200 pour éviter l'énumération d'emails.
    /// </summary>
    /// <response code="200">Si l'email existe, un code a été envoyé.</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _authService.SendPasswordResetOtpAsync(request);
        return Ok(new { message = "Si cet email est enregistré, un code de réinitialisation a été envoyé." });
    }

    // ── POST /auth/reset-password ─────────────────────────────────────────────

    /// <summary>
    /// Réinitialise le mot de passe en validant le code OTP reçu par email.
    /// </summary>
    /// <response code="200">Mot de passe réinitialisé.</response>
    /// <response code="400">Code invalide, expiré ou déjà utilisé.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithOtpDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _authService.ResetPasswordWithOtpAsync(dto);
        return success
            ? Ok(new { message = "Mot de passe réinitialisé avec succès." })
            : BadRequest(new { message = "Code invalide, expiré ou déjà utilisé." });
    }

    // ── POST /auth/confirm-email ──────────────────────────────────────────────

    /// <summary>
    /// Vérifie l'adresse email via le code OTP reçu par email.
    /// Utilisé après inscription ou après changement d'adresse email.
    /// </summary>
    /// <response code="200">Email confirmé.</response>
    /// <response code="400">Code invalide ou expiré.</response>
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _authService.ConfirmEmailAsync(dto);
        return success
            ? Ok(new { message = "Adresse email confirmée avec succès." })
            : BadRequest(new { message = "Code invalide ou expiré." });
    }

    // ── POST /auth/admin/login ────────────────────────────────────────────────

    /// <summary>
    /// Connexion administrateur, consciente du "bootstrap" :
    /// - Si le compte n'a pas encore activé le 2FA → connexion directe avec
    ///   <c>requiresTwoFactorSetup: true</c>. Le frontend DOIT alors rediriger
    ///   immédiatement vers la page de configuration 2FA.
    /// - Si le 2FA est actif et qu'aucun <c>totpCode</c> n'est fourni →
    ///   401 avec <c>totpRequired: true</c> (pas une erreur d'identifiants ;
    ///   le frontend doit simplement afficher le champ de code).
    /// - Si le 2FA est actif et le code est fourni → vérifié normalement.
    /// </summary>
    /// <response code="200">Connexion réussie (avec ou sans 2FA déjà actif).</response>
    /// <response code="401">Identifiants invalides, compte désactivé, rôle insuffisant, ou code TOTP manquant/invalide.</response>
    [HttpPost("admin/login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AdminLogin([FromBody] AdminTwoFactorLoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.AdminLoginWithTwoFactorAsync(dto);

        if (!result.Success)
        {
            return Unauthorized(new
            {
                message      = result.ErrorMessage,
                totpRequired = result.TotpRequired,
            });
        }

        AppendAuthCookies(result);
        return Ok(new
        {
            message                 = result.RequiresTwoFactorSetup
                ? "Connexion réussie. Configuration du 2FA requise."
                : "Connexion administrateur réussie.",
            requiresTwoFactorSetup  = result.RequiresTwoFactorSetup,
        });
    }

    // ── POST /auth/2fa/setup ──────────────────────────────────────────────────

    /// <summary>
    /// Génère un secret TOTP et retourne l'URL otpauth:// pour configurer
    /// Google Authenticator / Authy. Réservé aux admins.
    /// </summary>
    /// <response code="200">Secret et URL générés.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="403">Rôle insuffisant.</response>
    [Authorize]
    [HttpPost("2fa/setup")]
    [ProducesResponseType(typeof(TwoFactorSetupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetupTwoFactor()
    {
        var userId = ClaimsHelper.GetUserId(User);
        var setup  = await _authService.SetupTwoFactorAsync(userId);
        return Ok(setup);
    }

    // ── POST /auth/2fa/confirm ────────────────────────────────────────────────

    /// <summary>
    /// Confirme l'activation du 2FA en soumettant un premier code TOTP valide.
    /// </summary>
    /// <response code="200">2FA activé.</response>
    /// <response code="400">Code TOTP invalide.</response>
    [Authorize]
    [HttpPost("2fa/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmTwoFactor([FromBody] TwoFactorConfirmDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId  = ClaimsHelper.GetUserId(User);
        var success = await _authService.ConfirmTwoFactorAsync(userId, dto.TotpCode);
        return success
            ? Ok(new { message = "Authentification à deux facteurs activée avec succès." })
            : BadRequest(new { message = "Code TOTP invalide." });
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void AppendAuthCookies(AuthResultDto result)
    {
        AppendAuthCookie("cyna_token",         result.Token!,        minutes: 15);
        AppendAuthCookie("cyna_refresh_token", result.RefreshToken!, minutes: 1440);
    }

    private void AppendAuthCookie(string key, string value, int minutes)
    {
        var options = GetCookieOptions();
        options.Expires = DateTime.UtcNow.AddMinutes(minutes);
        Response.Cookies.Append(key, value, options);
    }

    private CookieOptions GetCookieOptions(bool expired = false) => new()
    {
        HttpOnly = true,
        Secure   = true,          // Set to true in production (HTTPS)
        SameSite = SameSiteMode.Lax,
        Domain = ".projet-cyna.fr", // Set to your domain
        Path     = "/",
        Expires  = expired ? DateTime.UtcNow.AddDays(-1) : null
    };
}