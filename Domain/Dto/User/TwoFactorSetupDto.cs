namespace Domain.Dto.User;

/// <summary>
/// Résultat de la configuration 2FA pour un administrateur.
/// </summary>
public class TwoFactorSetupDto
{
    /// <summary>Clé secrète TOTP en base32, à scanner dans Google Authenticator / Authy.</summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>URL otpauth:// à encoder en QR code côté client.</summary>
    public string OtpAuthUrl { get; set; } = string.Empty;
}