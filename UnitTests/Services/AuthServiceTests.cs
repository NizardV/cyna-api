using System;
using System.Threading;
using System.Threading.Tasks;

using Application.Interfaces;
using Application.Services;

using Domain.Dto.User;
using Domain.Entities;
using Domain.Entities.AuthCodes;

using Infrastructure.Interfaces;

using Microsoft.Extensions.Configuration;

using Moq;

using Resend;

using Tools; // Pour l'extension GetHash() et VerifyHashProvided()

using Xunit;

namespace Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITokenGenerator> _mockTokenGenerator;
    private readonly Mock<IResend> _mockResend;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // 1. Initialisation des faux services de base
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTokenGenerator = new Mock<ITokenGenerator>();

        // 2. Initialisation des faux services pour l'EmailHelper
        _mockResend = new Mock<IResend>();
        _mockConfig = new Mock<IConfiguration>();

        // On simule le retour de la configuration pour éviter un crash dans le constructeur de EmailHelper
        _mockConfig.Setup(c => c["Resend:From"]).Returns("test-sender@test.com");

        // 3. Création du VRAI EmailHelper, mais on lui passe les faux IResend et IConfiguration
        var realEmailHelper = new EmailHelper(_mockResend.Object, _mockConfig.Object);

        // 4. Création du service à tester avec le vrai EmailHelper sécurisé
        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockTokenGenerator.Object,
            realEmailHelper
        );
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
    public async Task LoginAsync_WhenUserIsDisabled_ReturnsError()
    {
        // ARRANGE
        var request = new LoginRequestDto { Email = "jean@test.com", Password = "Password123!" };
        var fakeUser = new User
        {
            Email = request.Email,
            PasswordHash = request.Password.GetHash(), // Simule un hash valide
            IsDisabled = true
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email)).ReturnsAsync(fakeUser);

        // ACT
        var result = await _authService.LoginAsync(request);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal("Ce compte a été désactivé. Contactez l'administrateur.", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_WhenAdminHas2FAEnabled_ReturnsError()
    {
        // ARRANGE
        var request = new LoginRequestDto { Email = "admin@test.com", Password = "Password123!" };
        var fakeUser = new User
        {
            Email = request.Email,
            PasswordHash = request.Password.GetHash(),
            Role = UserRole.Admin,
            TwoFactorEnabled = true
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email)).ReturnsAsync(fakeUser);

        // ACT
        var result = await _authService.LoginAsync(request);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal("Les comptes administrateur avec 2FA activé doivent utiliser la connexion administrateur.", result.ErrorMessage);
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
            PasswordHash = request.Password.GetHash(), // Doit passer VerifyHashProvided()
            Role = UserRole.User,
            IsDisabled = false
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

        // On vérifie qu'on n'a JAMAIS appelé AddAsync
        _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenValidRequest_AddsUserAndSendsVerificationEmail()
    {
        // ARRANGE
        var request = new RegisterRequestDto { Email = "nouveau@test.com", Password = "Pwd", FirstName = "A", LastName = "B" };
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email)).ReturnsAsync((User)null);

        // 1. On crée l'objet en mémoire sans appeler son constructeur
        var fakeResponse = (ResendResponse<Guid>)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ResendResponse<Guid>));

        // 2. On accède au champ privé caché généré par C# (<Content>k__BackingField) pour y injecter un faux Guid
        var backingField = typeof(ResendResponse<Guid>).GetField("<Content>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        backingField?.SetValue(fakeResponse, Guid.NewGuid());

        // 3. On demande à Moq de retourner cet objet manipulé
        _mockResend.Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(fakeResponse);
        // ACT
        var result = await _authService.RegisterAsync(request);

        // ASSERT
        Assert.True(result.Success);

        _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);

        // On peut vérifier que IResend a bien été appelé par le EmailHelper !
        _mockResend.Verify(r => r.EmailSendAsync(It.Is<EmailMessage>(m => m.To.Contains(request.Email)), It.IsAny<CancellationToken>()), Times.Once);
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
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1), // Expiré
            IsDisabled = false
        };

        _mockUserRepository.Setup(repo => repo.GetByRefreshTokenAsync(request.RefreshToken)).ReturnsAsync(expiredUser);

        // ACT
        var result = await _authService.ResetTokenAsync(request);

        // ASSERT
        Assert.Null(result); // Le service doit bloquer la requête
    }

    [Fact]
    public async Task ResetTokenAsync_WhenUserIsDisabled_ReturnsNull()
    {
        // ARRANGE
        var request = new RefreshTokenRequestDto { RefreshToken = "valid_token" };
        var disabledUser = new User
        {
            RefreshToken = "valid_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
            IsDisabled = true // Désactivé
        };

        _mockUserRepository.Setup(repo => repo.GetByRefreshTokenAsync(request.RefreshToken)).ReturnsAsync(disabledUser);

        // ACT
        var result = await _authService.ResetTokenAsync(request);

        // ASSERT
        Assert.Null(result);
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