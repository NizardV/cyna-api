using Application.Interfaces;

namespace Api.Security;

/// <summary>
/// Implémentation temporaire — retourne un userId fixe tant que l'auth JWT n'est pas active.
/// À remplacer par JwtCurrentUserService dans Program.cs le jour J.
/// </summary>
public class MockCurrentUserService : ICurrentUserService
{
    public int UserId => 1;
}
