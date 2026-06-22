using System;
using System.Threading.Tasks;

using Application.Interfaces;
using Application.Services;

using Domain.Dto.User;
using Domain.Entities;

using Infrastructure.Interfaces;

using Moq;

using Tools; // Pour l'extension GetHash()

using Xunit;

namespace Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITokenGenerator> _mockTokenGenerator;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // 1. Initialisation des faux services (Mocks)
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTokenGenerator = new Mock<ITokenGenerator>();

        // 2. Création du vrai service à tester
        _authService = new AuthService(_mockUserRepository.Object, _mockTokenGenerator.Object);
    }

    // =========================================================================
    // TESTS POUR : LoginAsync
    // =========================================================================

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsError()
    {
        // ARRANGE
        var request = new LoginRequestDto { Email = "inconnu@test.com", Password = "Password123!" };
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email)).ReturnsAsync((User)null);

        // ACT
        var result = await _authService.LoginAsync(request);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal("Identifiants invalides.", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_WhenValidCredentials_ReturnsTokensAndUpdatesUser()
    {
        // ARRANGE
        var request = new LoginRequestDto { Email = "jean@test.com", Password = "Password123!" };

        var fakeUser = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = request.Password.GetHash() // On simule le vrai hashage pour que ça corresponde !
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email)).ReturnsAsync(fakeUser);
        _mockTokenGenerator.Setup(t => t.GenerateToken(fakeUser)).Returns("fake_jwt_token");

        // ACT
        var result = await _authService.LoginAsync(request);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal("fake_jwt_token", result.Token);
        Assert.NotNull(result.RefreshToken);

        // On vérifie que la méthode UpdateAsync a bien été appelée UNE FOIS pour sauvegarder le nouveau token
        _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    // =========================================================================
    // TESTS POUR : RegisterAsync
    // =========================================================================

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ReturnsError()
    {
        // ARRANGE
        var request = new RegisterRequestDto { Email = "existant@test.com", Password = "Pwd", FirstName = "A", LastName = "B" };
        var existingUser = new User { Email = request.Email };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email)).ReturnsAsync(existingUser);

        // ACT
        var result = await _authService.RegisterAsync(request);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal("Email déjà utilisé.", result.ErrorMessage);

        // On vérifie qu'on n'a JAMAIS appelé AddAsync (sécurité)
        _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
    }

    // =========================================================================
    // TESTS POUR : ResetTokenAsync
    // =========================================================================

    [Fact]
    public async Task ResetTokenAsync_WhenTokenIsExpired_ReturnsNull()
    {
        // ARRANGE
        var request = new RefreshTokenRequestDto { RefreshToken = "old_token" };
        var expiredUser = new User
        {
            RefreshToken = "old_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1) // Expiré depuis hier !
        };

        _mockUserRepository.Setup(repo => repo.GetByRefreshTokenAsync(request.RefreshToken)).ReturnsAsync(expiredUser);

        // ACT
        var result = await _authService.ResetTokenAsync(request);

        // ASSERT
        Assert.Null(result); // Le service doit bloquer la requête
    }

    // =========================================================================
    // TESTS POUR : LogoutAsync
    // =========================================================================

    [Fact]
    public async Task LogoutAsync_WhenValidToken_ClearsTokenDataAndReturnsTrue()
    {
        // ARRANGE
        string validToken = "my_refresh_token";
        var user = new User
        {
            RefreshToken = validToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _mockUserRepository.Setup(repo => repo.GetByRefreshTokenAsync(validToken)).ReturnsAsync(user);

        // ACT
        var result = await _authService.LogoutAsync(validToken);

        // ASSERT
        Assert.True(result);
        Assert.Null(user.RefreshToken); // On vérifie que le token a été effacé de l'objet
        Assert.Null(user.RefreshTokenExpiryTime);

        // On vérifie que la sauvegarde en BDD a bien été déclenchée
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }
}