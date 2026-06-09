namespace Infrastructure.Interfaces;

using Domain.Entities;

public interface ITokenGenerator
{
    /// <summary>
    /// Génère un token d'authentification pour un utilisateur donné.
    /// </summary>
    /// <param name="user">L'utilisateur pour lequel générer le token.</param>
    /// <returns>Le token d'authentification.</returns>
    string GenerateToken(User user);
}