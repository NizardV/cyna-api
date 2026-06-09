namespace Api.Security;

using System.Security.Claims;

using Application.Interfaces;

public class JwtCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null || !int.TryParse(claim.Value, out var id))
                throw new UnauthorizedAccessException("Identifiant utilisateur introuvable dans le token.");

            return id;
        }
    }
}