namespace Domain.Dto.User;

/// <summary>
/// Résultat d'une opération d'authentification (connexion, inscription ou renouvellement de token).
/// </summary>
public class AuthResultDto
{
    /// <summary>Indique si l'opération a réussi.</summary>
    public bool Success { get; set; }

    /// <summary>L'access token JWT généré, ou <c>null</c> en cas d'échec ou à l'inscription.</summary>
    public string? Token { get; set; }

    /// <summary>Le refresh token généré, ou <c>null</c> en cas d'échec ou à l'inscription.</summary>
    public string? RefreshToken { get; set; }

    /// <summary>Le message d'erreur en cas d'échec, ou <c>null</c> en cas de succès.</summary>
    public string? ErrorMessage { get; set; }
}
