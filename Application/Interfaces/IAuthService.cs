using Domain.Dto.User;

namespace Application.Interfaces;

/// <summary>
/// Interface du service d'authentification.
/// Orchestre la connexion, l'inscription, le renouvellement de token et la déconnexion.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authentifie un utilisateur avec son email et son mot de passe.
    /// Génère un access token et un refresh token en cas de succès.
    /// </summary>
    /// <param name="request">L'email et le mot de passe de l'utilisateur.</param>
    /// <returns>Le résultat d'authentification avec les tokens, ou un échec avec un message d'erreur.</returns>
    Task<AuthResultDto?> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Crée un nouveau compte utilisateur après vérification de l'unicité de l'email.
    /// </summary>
    /// <param name="request">Les informations d'inscription.</param>
    /// <returns>Le résultat d'inscription (succès ou erreur si l'email est déjà utilisé).</returns>
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Renouvelle l'access token à partir d'un refresh token valide et non expiré.
    /// </summary>
    /// <param name="request">Le refresh token à valider.</param>
    /// <returns>De nouveaux tokens en cas de succès, ou <c>null</c> si le token est invalide ou expiré.</returns>
    Task<AuthResultDto?> ResetTokenAsync(RefreshTokenRequestDto request);

    /// <summary>
    /// Invalide le refresh token de l'utilisateur en base de données.
    /// </summary>
    /// <param name="refreshToken">Le refresh token à invalider.</param>
    /// <returns><c>true</c> si le token a été invalidé, <c>false</c> si l'utilisateur est introuvable.</returns>
    Task<bool> LogoutAsync(string refreshToken);
}