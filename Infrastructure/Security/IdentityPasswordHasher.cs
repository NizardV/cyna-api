using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Security;

using Interfaces;

public class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _inner = new();

    public string HashPassword(string password)
    {
        return _inner.HashPassword(user: null!, password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _inner.VerifyHashedPassword(user: null!, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}
