using Domain.Entities;

namespace Application.Interfaces;

public interface ITokenGenerator
{
    string GenerateToken(User user);
}
