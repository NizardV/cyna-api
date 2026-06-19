namespace Tools;

using System.Security.Claims;

/// <summary>
/// Classe utilitaire pour extraire les informations de l'utilisateur connecté depuis les claims JWT.
/// </summary>
public static class ClaimsHelper
{
    /// <summary>
    /// Extrait l'identifiant de l'utilisateur depuis les claims du token JWT.
    /// </summary>
    /// <param name="user">L'objet ClaimsPrincipal issu du contexte HTTP.</param>
    /// <returns>L'identifiant de l'utilisateur.</returns>
    /// <exception cref="UnauthorizedAccessException">Si le claim d'identifiant est absent ou invalide.</exception>
    public static int GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("id")
                 ?? user.FindFirst(ClaimTypes.NameIdentifier)
                 ?? user.FindFirst("sub");

        if (claim == null || !int.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Identifiant utilisateur introuvable dans le token.");
        }

        return userId;
    }
}