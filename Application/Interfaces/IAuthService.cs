using Domain.Dto.User;

namespace Application.Interfaces;

/// <summary>
/// Interface du service d'authentification.
/// </summary>
public interface IAuthService
{
    /// <summary>Authentifie un utilisateur. Retourne un échec si le compte est désactivé.</summary>
    Task<AuthResultDto> LoginAsync(LoginRequestDto request);

    Task<AuthResultDto> RegisterAsync(RegisterRequestDto request);

    Task<AuthResultDto?> ResetTokenAsync(RefreshTokenRequestDto request);

    Task<bool> LogoutAsync(string refreshToken);

    // ── Password reset via OTP ────────────────────────────────────────────────

    /// <summary>
    /// Envoie un code OTP de réinitialisation de mot de passe à l'adresse email indiquée.
    /// Ne révèle pas si l'email existe (anti-énumération).
    /// </summary>
    Task SendPasswordResetOtpAsync(ForgotPasswordRequestDto request);

    /// <summary>
    /// Vérifie le code OTP et applique le nouveau mot de passe si valide.
    /// </summary>
    Task<bool> ResetPasswordWithOtpAsync(ResetPasswordWithOtpDto dto);

    // ── Email verification ────────────────────────────────────────────────────

    /// <summary>
    /// Vérifie le code OTP reçu par email et marque l'adresse comme vérifiée.
    /// </summary>
    Task<bool> ConfirmEmailAsync(ConfirmEmailDto dto);

    // ── Admin 2FA ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Génère et persiste un secret TOTP pour l'admin connecté.
    /// Retourne la clé secrète et l'URL otpauth:// pour le QR code.
    /// </summary>
    Task<TwoFactorSetupDto> SetupTwoFactorAsync(int adminUserId);

    /// <summary>
    /// Confirme l'activation du 2FA en vérifiant un code TOTP.
    /// </summary>
    Task<bool> ConfirmTwoFactorAsync(int adminUserId, string totpCode);

    /// <summary>
    /// Authentifie un administrateur. Toujours retourne un résultat structuré
    /// (jamais null) — voir <see cref="AuthResultDto.RequiresTwoFactorSetup"/>
    /// et <see cref="AuthResultDto.TotpRequired"/> pour distinguer les cas :
    /// bootstrap (1ère connexion, pas encore de 2FA), code manquant, code
    /// invalide, ou succès complet.
    /// </summary>
    Task<AuthResultDto> AdminLoginWithTwoFactorAsync(AdminTwoFactorLoginDto dto);

}