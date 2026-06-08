using System.ComponentModel;
using System.Reflection;

using Microsoft.AspNetCore.Identity;

namespace Tools;

public static class HashExstension
{
    private static readonly PasswordHasher<object> _inner = new();

    public static string GetHash(this string value)
    {
        return _inner.HashPassword(user: null!, value);
    }

    public static bool VerifyHashProvided(this string providedPassword, string hashedPassword)
    {
        var result = _inner.VerifyHashedPassword(user: null!, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}