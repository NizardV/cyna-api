using Application.Interfaces;

using Microsoft.AspNetCore.Http;

using Tools;

namespace Api.Security;

/// <summary>
/// Implémentation JWT — lit l'userId depuis les claims du token.
/// Activer dans Program.cs quand l'auth sera fonctionnelle.
/// </summary>
public class JwtCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public JwtCurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public int UserId => ClaimsHelper.GetUserId(_http.HttpContext!.User);
}
