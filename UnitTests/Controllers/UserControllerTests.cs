using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Api.Controllers;

using Application.Interfaces;

using Domain.Dto.User;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;

namespace Api.Tests.Controllers;

public class UserControllerTests
{
    // 1. Déclaration de nos Mocks (Faux services)
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<ISubscriptionService> _mockSubscriptionService;

    // Le contrôleur à tester
    private readonly UserController _controller;

    public UserControllerTests()
    {
        // 2. Instanciation des Mocks
        _mockUserService = new Mock<IUserService>();
        _mockOrderService = new Mock<IOrderService>();
        _mockSubscriptionService = new Mock<ISubscriptionService>();

        // 3. Instanciation du contrôleur avec les faux services
        _controller = new UserController(
            _mockUserService.Object,
            _mockOrderService.Object,
            _mockSubscriptionService.Object
        );
    }

    // =========================================================================
    // TESTS POUR: GetSubscriptions()
    // =========================================================================

    [Fact]
    public async Task GetSubscriptions_WhenUserIsAuthenticated_ReturnsOkWithData()
    {
        // ARRANGE
        int fakeUserId = 42;
        SetupMockUser(fakeUserId); // Utilisation d'une méthode Helper (voir tout en bas)

        var fakeSubscriptions = new List<SubscriptionDto>
        {
            new SubscriptionDto { Id = 1, ProductName = "Shield EDR", Status = "Active" },
            new SubscriptionDto { Id = 2, ProductName = "Sentinel SIEM", Status = "Expired" }
        };

        _mockSubscriptionService
            .Setup(s => s.GetUserSubscriptionsAsync(fakeUserId))
            .ReturnsAsync(fakeSubscriptions);

        // ACT
        var result = await _controller.GetSubscriptions();

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSubscriptions = Assert.IsAssignableFrom<IEnumerable<SubscriptionDto>>(okResult.Value);
        Assert.Equal(2, returnedSubscriptions.Count());
    }

    [Fact]
    public async Task GetSubscriptions_WhenServiceThrowsUnauthorized_ReturnsUnauthorized()
    {
        // ARRANGE
        int fakeUserId = 99;
        SetupMockUser(fakeUserId);

        _mockSubscriptionService
            .Setup(s => s.GetUserSubscriptionsAsync(fakeUserId))
            .ThrowsAsync(new UnauthorizedAccessException("Session expirée"));

        // ACT
        var result = await _controller.GetSubscriptions();

        // ASSERT
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    // =========================================================================
    // TESTS POUR: GetProfile()
    // =========================================================================

    [Fact]
    public async Task GetProfile_WhenUserExists_ReturnsOkWithProfile()
    {
        // ARRANGE
        int fakeUserId = 123;
        SetupMockUser(fakeUserId);

        var expectedProfile = new UserProfileDto
        {
            Id = fakeUserId,
            Email = "jean.dupont@cyna.fr",
            FirstName = "Jean",
            LastName = "Dupont"
        };

        _mockUserService
            .Setup(s => s.GetProfileAsync(fakeUserId))
            .ReturnsAsync(expectedProfile);

        // ACT
        var result = await _controller.GetProfile();

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProfile = Assert.IsType<UserProfileDto>(okResult.Value);
        Assert.Equal("jean.dupont@cyna.fr", returnedProfile.Email);
    }

    [Fact]
    public async Task GetProfile_WhenUserDoesNotExist_ReturnsNotFound()
    {
        // ARRANGE
        int fakeUserId = 999;
        SetupMockUser(fakeUserId);

        _mockUserService
            .Setup(s => s.GetProfileAsync(fakeUserId))
            .ThrowsAsync(new KeyNotFoundException("Utilisateur introuvable"));

        // ACT
        var result = await _controller.GetProfile();

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // =========================================================================
    // MÉTHODES HELPER (Pour éviter de se répéter dans chaque test)
    // =========================================================================

    /// <summary>
    /// Simule un utilisateur connecté dans le contexte HTTP du contrôleur.
    /// Nécessaire car nos routes utilisent [Authorize] et ClaimsHelper.GetUserId().
    /// </summary>
    private void SetupMockUser(int userId)
    {
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userClaims }
        };
    }
}