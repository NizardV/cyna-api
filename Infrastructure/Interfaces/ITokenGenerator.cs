namespace Infrastructure.Interfaces;

using Domain.Entities;

/// <summary>
/// Interface du générateur de tokens JWT.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Génère un access token JWT signé pour l'utilisateur donné.
    /// Le token contient les claims : id, firstName, lastName, email, role.
    /// </summary>
    /// <param name="user">L'utilisateur pour lequel générer le token.</param>
    /// <returns>Le token JWT signé sous forme de chaîne.</returns>
    string GenerateToken(User user);
}