using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Application.Services;

using Domain.Dto.Cart;
using Domain.Entities.Catalogue;
using Domain.Entities.OrdersAndSubscriptions;

using Infrastructure.Entities;
using Infrastructure.Interfaces;

using Moq;

using Tools;

using Xunit;

namespace Api.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly CartService _cartService;

    public CartServiceTests()
    {
        // 1. On Mock le Repository (la fausse base de données)
        _mockCartRepository = new Mock<ICartRepository>();

        // 2. On instancie le VRAI service en lui passant la fausse base de données
        _cartService = new CartService(_mockCartRepository.Object);
    }

    // =========================================================================
    // TEST 1 : Quantités à zéro
    // =========================================================================
    [Fact]
    public async Task AddOrUpdateCartItemAsync_WhenQuantitiesAreZero_ThrowsArgumentException()
    {
        // ARRANGE
        int userId = 42;
        var requestDto = new AddCartItemRequestDto { PricingPlanId = 1, QuantityUsers = 0, QuantityDevices = 0 };

        // ACT & ASSERT
        // Comme on s'attend à ce que la méthode plante, on utilise Assert.ThrowsAsync
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _cartService.AddOrUpdateCartItemAsync(userId, requestDto));

        // On peut même vérifier que le message d'erreur est bien celui attendu
        Assert.Equal("Au moins une quantité (utilisateurs ou appareils) doit être supérieure à zéro.", exception.Message);
    }

    // =========================================================================
    // TEST 2 : Plan tarifaire introuvable
    // =========================================================================
    [Fact]
    public async Task AddOrUpdateCartItemAsync_WhenPlanNotFound_ThrowsKeyNotFoundException()
    {
        // ARRANGE
        int userId = 42;
        var requestDto = new AddCartItemRequestDto { PricingPlanId = 999, QuantityUsers = 5, QuantityDevices = 0 };

        // On simule une base de données qui ne trouve rien (renvoie null)
        _mockCartRepository
            .Setup(repo => repo.GetPricingPlanWithTiersAsync(999))
            .ReturnsAsync((PricingPlan)null);

        // ACT & ASSERT
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _cartService.AddOrUpdateCartItemAsync(userId, requestDto));

        Assert.Contains("introuvable", exception.Message);
    }

    // =========================================================================
    // TEST 3 : Calcul correct des prix et de la TVA
    // =========================================================================
    [Fact]
    public async Task AddOrUpdateCartItemAsync_WhenValid_CalculatesPricesAndTaxCorrectly()
    {
        // ARRANGE
        int userId = 42;
        var requestDto = new AddCartItemRequestDto { PricingPlanId = 1, QuantityUsers = 2, QuantityDevices = 3 };

        // On fabrique un faux plan tarifaire en mémoire
        var fakePlan = new PricingPlan
        {
            Id = 1,
            ProductId = 100,
            BillingPeriod = BillingPeriod.Monthly,
            Product = new Product
            {
                Id = 100,
                Slug = "produit-test",
                Translations = new List<ProductTranslation>
                {
                    new ProductTranslation { Locale = LocaleLang.Fr, Name = "Shield EDR" }
                }
            },
            PricingTiers = new List<PricingTier>
            {
                // Palier Utilisateurs : 10€ / unité
                new PricingTier { unitType = BillingUnit.User, minQuantity = 1, maxQuantity = 100, PricePerUnit = 10m },
                // Palier Appareils : 5€ / unité
                new PricingTier { unitType = BillingUnit.Device, minQuantity = 1, maxQuantity = 50, PricePerUnit = 5m }
            }
        };

        // On fabrique un faux CartItem qui sera renvoyé après la sauvegarde
        var fakeCartItem = new CartItem
        {
            Id = 99,
            UserId = userId,
            PricingPlanId = 1,
            PricingPlan = fakePlan,
            QuantityUsers = 2,
            QuantityDevices = 3
        };

        // Configuration des Mocks
        _mockCartRepository.Setup(r => r.GetPricingPlanWithTiersAsync(1)).ReturnsAsync(fakePlan);

        _mockCartRepository.Setup(r => r.UpsertCartItemAsync(userId, 100, 1, 2, 3)).ReturnsAsync(fakeCartItem);

        _mockCartRepository.Setup(r => r.GetCartItemsAsync(userId)).ReturnsAsync(new List<CartItem> { fakeCartItem });


        // ACT
        var result = await _cartService.AddOrUpdateCartItemAsync(userId, requestDto);


        // ASSERT
        // 1. Vérification du produit
        Assert.NotNull(result);
        Assert.Equal("Shield EDR", result.Item.ProductName);
        Assert.Equal(10m, result.Item.UnitPriceUsers);
        Assert.Equal(5m, result.Item.UnitPriceDevices);

        // 2. Vérification de la ligne de prix (2 utilisateurs * 10€) + (3 appareils * 5€) = 20 + 15 = 35€
        Assert.Equal(35m, result.Item.LineTotal);

        // 3. Vérification du panier global (Subtotal = 35€, Taxe = 20% de 35 = 7€, Total = 42€)
        Assert.Equal(35m, result.CartSummary.Subtotal);
        Assert.Equal(7m, result.CartSummary.TaxAmount);
        Assert.Equal(42m, result.CartSummary.Total);
    }

    // ====================================
    // TEST 4 : Paliers de prix dégressifs 
    // ====================================
    [Fact]
    public async Task AddOrUpdateCartItemAsync_WithMultipleTiers_AppliesCorrectTierPrice()
    {
        // ARRANGE
        int userId = 42;
        int quantityToBuy = 60; // On achète 60 licences, on doit donc tomber dans le 2ème palier !
        var requestDto = new AddCartItemRequestDto { PricingPlanId = 1, QuantityUsers = quantityToBuy, QuantityDevices = 0 };

        var fakePlan = new PricingPlan
        {
            Id = 1,
            ProductId = 100,
            Product = new Product
            {
                Translations = new List<ProductTranslation> { new ProductTranslation { Locale = LocaleLang.Fr, Name = "Licence Dégressive" } }
            },
            PricingTiers = new List<PricingTier>
            {
                // PALIER 1 : 10€ (Si on en achète entre 1 et 50)
                new PricingTier { unitType = BillingUnit.User, minQuantity = 1, maxQuantity = 50, PricePerUnit = 10m },
                
                // PALIER 2 : 8€ (Si on en achète entre 51 et 100) -> C'EST CELUI QU'ON VISE !
                new PricingTier { unitType = BillingUnit.User, minQuantity = 51, maxQuantity = 100, PricePerUnit = 8m }
            }
        };

        var fakeCartItem = new CartItem
        {
            UserId = userId,
            PricingPlanId = 1,
            PricingPlan = fakePlan,
            QuantityUsers = quantityToBuy,
            QuantityDevices = 0
        };

        // Configuration des Mocks
        _mockCartRepository.Setup(r => r.GetPricingPlanWithTiersAsync(1)).ReturnsAsync(fakePlan);
        // On utilise It.IsAny<int>() car le ProductId ne nous intéresse pas pour ce test précis
        _mockCartRepository.Setup(r => r.UpsertCartItemAsync(userId, It.IsAny<int>(), 1, quantityToBuy, 0)).ReturnsAsync(fakeCartItem);
        _mockCartRepository.Setup(r => r.GetCartItemsAsync(userId)).ReturnsAsync(new List<CartItem> { fakeCartItem });

        // ACT
        var result = await _cartService.AddOrUpdateCartItemAsync(userId, requestDto);

        // ASSERT
        // Vérification cruciale : Le service a-t-il bien choisi le prix de 8€ au lieu de 10€ ?
        Assert.Equal(8m, result.Item.UnitPriceUsers);

        // La ligne totale doit donc être : 60 utilisateurs * 8€ = 480€ (et non 600€)
        Assert.Equal(480m, result.Item.LineTotal);
    }
}