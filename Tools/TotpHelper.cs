namespace Tools;

using OtpNet;

/// <summary>
/// Utilitaires pour la génération et la vérification TOTP (RFC 6238).
/// Nécessite le package NuGet <c>OtpNet</c>.
/// </summary>
public static class TotpHelper
{
    /// <summary>
    /// Génère une clé secrète aléatoire en base32 pour le 2FA.
    /// </summary>
    public static string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20); // 160 bits
        return Base32Encoding.ToString(key);
    }

    /// <summary>
    /// Construit une URL otpauth:// compatible Google Authenticator / Authy.
    /// </summary>
    /// <param name="email">Label affiché dans l'application.</param>
    /// <param name="secret">Clé secrète en base32.</param>
    /// <param name="issuer">Nom de l'application (ex : "Cyna").</param>
    public static string BuildOtpAuthUrl(string email, string secret, string issuer = "Cyna")
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail  = Uri.EscapeDataString(email);
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    /// <summary>
    /// Vérifie un code TOTP à 6 chiffres.
    /// Autorise une fenêtre de ±1 période (30 s) pour compenser la dérive d'horloge.
    /// </summary>
    /// <param name="secret">Clé secrète en base32 de l'utilisateur.</param>
    /// <param name="code">Code à 6 chiffres saisi par l'utilisateur.</param>
    public static bool Verify(string secret, string code)
    {
        try
        {
            var key  = Base32Encoding.ToBytes(secret);
            var totp = new Totp(key);
            return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }
}