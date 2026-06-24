using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Api.Controllers;

using Application.Interfaces.Services;

using Domain.Dto.Cart;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;
using Xunit.Abstractions;

namespace Api.Tests.Controllers;

public class CartControllerTests
{
    private readonly Mock<ICartService> _mockCartService;
    private readonly ITestOutputHelper _output;
    private readonly CartController _controller;

    public CartControllerTests(ITestOutputHelper output)
    {
        _output = output;
        _mockCartService = new Mock<ICartService>();

        // Instanciation du contrôleur avec le faux CartService
        _controller = new CartController(_mockCartService.Object);

        _output.WriteLine("🛠️ Mocks et CartController initialisés.");
    }

    // =========================================================================
    // TESTS POUR: AddToCart()
    // =========================================================================

    [Fact]
    public async Task AddToCart_WhenValidRequest_ReturnsCreated()
    {
        _output.WriteLine("▶️ DÉBUT DU TEST : AddToCart avec succès (201 Created)");

        // 1. ARRANGE
        int fakeUserId = 42;
        SetupMockUser(fakeUserId);

        var requestDto = new AddCartItemRequestDto
        {
            PricingPlanId = 1,
            QuantityUsers = 5,
            QuantityDevices = 0
        };

        var expectedResult = new CartResultDto { CartId = fakeUserId };

        // Configuration du Mock : "Si on t'appelle avec ces paramètres, renvoie ce résultat"
        _mockCartService
            .Setup(s => s.AddOrUpdateCartItemAsync(fakeUserId, requestDto))
            .ReturnsAsync(expectedResult);

        // 2. ACT
        _output.WriteLine("🚀 ACT : Exécution de _controller.AddToCart()...");
        var result = await _controller.AddToCart(requestDto);

        // 3. ASSERT
        _output.WriteLine("✅ ASSERT : Vérification que le résultat est un 201 Created...");
        var createdResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(expectedResult, createdResult.Value);
    }

    [Fact]
    public async Task AddToCart_WhenQuantitiesAreZero_ReturnsBadRequest()
    {
        _output.WriteLine("▶️ DÉBUT DU TEST : AddToCart avec quantités à zéro (400 BadRequest)");

        // 1. ARRANGE
        int fakeUserId = 42;
        SetupMockUser(fakeUserId);

        var requestDto = new AddCartItemRequestDto { PricingPlanId = 1, QuantityUsers = 0, QuantityDevices = 0 };

        // On simule l'ArgumentException que ton service lève quand les quantités sont à 0
        _mockCartService
            .Setup(s => s.AddOrUpdateCartItemAsync(fakeUserId, requestDto))
            .ThrowsAsync(new ArgumentException("Au moins une quantité (utilisateurs ou appareils) doit être supérieure à zéro."));

        // 2. ACT
        _output.WriteLine("🚀 ACT : Exécution de _controller.AddToCart()...");
        var result = await _controller.AddToCart(requestDto);

        // 3. ASSERT
        _output.WriteLine("✅ ASSERT : Vérification que le contrôleur a intercepté l'erreur en 400 BadRequest...");
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task AddToCart_WhenPlanNotFound_ReturnsNotFound()
    {
        _output.WriteLine("▶️ DÉBUT DU TEST : AddToCart avec plan inexistant (404 NotFound)");

        // 1. ARRANGE
        int fakeUserId = 42;
        SetupMockUser(fakeUserId);

        var requestDto = new AddCartItemRequestDto { PricingPlanId = 999, QuantityUsers = 5, QuantityDevices = 0 };

        // On simule la KeyNotFoundException que ton service lève quand le plan n'existe pas
        _mockCartService
            .Setup(s => s.AddOrUpdateCartItemAsync(fakeUserId, requestDto))
            .ThrowsAsync(new KeyNotFoundException($"Plan tarifaire 999 introuvable."));

        // 2. ACT
        _output.WriteLine("🚀 ACT : Exécution de _controller.AddToCart()...");
        var result = await _controller.AddToCart(requestDto);

        // 3. ASSERT
        _output.WriteLine("✅ ASSERT : Vérification que le contrôleur a intercepté l'erreur en 404 NotFound...");
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    // =========================================================================
    // MÉTHODE HELPER
    // =========================================================================

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