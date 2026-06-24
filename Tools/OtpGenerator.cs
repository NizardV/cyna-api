namespace Tools;

using System.Security.Cryptography;

/// <summary>
/// Génère des codes OTP numériques cryptographiquement sécurisés.
/// </summary>
public static class OtpGenerator
{
    /// <summary>
    /// Génère un code OTP numérique à <paramref name="digits"/> chiffres.
    /// </summary>
    public static string Generate(int digits = 6)
    {
        // Génère un entier aléatoire dans [0, 10^digits[ via RNG
        var max   = (int)Math.Pow(10, digits);
        var value = RandomNumberGenerator.GetInt32(max);
        return value.ToString().PadLeft(digits, '0');
    }
}