using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Application.Services;

using Domain.Entities.Catalogue;

using Infrastructure.Interfaces;

using Moq;

using Tools;

using Xunit;

namespace Api.Tests.Services;

public class SearchServiceTests
{
    private readonly Mock<ISearchRepository> _mockSearchRepository;
    private readonly SearchService _searchService;

    public SearchServiceTests()
    {
        _mockSearchRepository = new Mock<ISearchRepository>();
        _searchService = new SearchService(_mockSearchRepository.Object);
    }

    // =========================================================================
    // TEST 1 : Nettoyage de la pagination
    // =========================================================================
    [Fact]
    public async Task GetProductsAsync_WithInvalidPagination_UsesDefaultValues()
    {
        // ARRANGE
        // On configure le mock pour qu'il renvoie une liste vide et 0 total peu importe les paramètres
        _mockSearchRepository
            .Setup(repo => repo.GetProductsAsync(It.IsAny<string>(), It.IsAny<List<int>>(), It.IsAny<decimal?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((new List<Product>(), 0));

        // ACT
        // On demande volontairement la page "-5" et une taille de "0"
        var result = await _searchService.GetProductsAsync(null, null, null, true, "asc", page: -5, pageSize: 0, "fr-FR");

        // ASSERT
        // Le service a dû corriger "-5" en "1" et "0" en "9" (DefaultPageSize)
        Assert.Equal(1, result.Page);
        Assert.Equal(9, result.PageSize);
        Assert.Equal(1, result.TotalPages); // Même avec 0 article, on affiche au moins 1 page
    }

    // =========================================================================
    // TEST 2 : Parsing des catégories (La chaîne de caractères)
    // =========================================================================
    [Fact]
    public async Task GetProductsAsync_WithCategoryIdsString_ParsesCorrectly()
    {
        // ARRANGE
        // Une liste d'IDs avec des espaces, des virgules en trop et du texte ("bad")
        string rawCategories = " 1, bad, 2 ,  , 3";
        IEnumerable<int> capturedCatIds = null;

        // On va "capturer" la liste d'IDs que le Service envoie au Repository
        _mockSearchRepository
            .Setup(repo => repo.GetProductsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>(), It.IsAny<decimal?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .Callback<string, IEnumerable<int>, decimal?, bool, string, int, int, string>((q, catIds, maxPrice, avail, sort, p, ps, loc) =>
            {
                capturedCatIds = catIds; // On sauvegarde ce que le service a calculé !
            })
            .ReturnsAsync((new List<Product>(), 0));

        // ACT
        await _searchService.GetProductsAsync(null, rawCategories, null, true, "asc", 1, 10, "fr-FR");

        // ASSERT
        Assert.NotNull(capturedCatIds);
        Assert.Equal(3, capturedCatIds.Count());
        Assert.Contains(1, capturedCatIds);
        Assert.Contains(2, capturedCatIds);
        Assert.Contains(3, capturedCatIds);
    }

    // =========================================================================
    // TEST 3 : Calcul du prix minimum (avec réductions) et Mapping complet
    // =========================================================================
    [Fact]
    public async Task GetProductsAsync_WithValidData_MapsToDtoAndCalculatesMinPrice()
    {
        // ARRANGE
        // On crée un faux produit complexe
        var fakeProduct = new Product
        {
            Id = 100,
            Slug = "nom-technique",
            Status = ProductStatus.Available,
            Translations = new List<ProductTranslation>
            {
                new ProductTranslation { Locale = LocaleLang.Fr, Name = "Produit Français", Description = "Desc FR" }
            },
            Images = new List<ProductImage> { new ProductImage { ImageUrl = "http://image.png" } },
            PricingPlans = new List<PricingPlan>
            {
                // Plan 1 : Prix 100€, pas de réduction -> 100€
                new PricingPlan
                {
                    DiscountPercent = 0,
                    PricingTiers = new List<PricingTier> { new PricingTier { PricePerUnit = 100m } }
                },
                // Plan 2 : Prix 60€, mais 50% de réduction ! -> 30€
                new PricingPlan
                {
                    DiscountPercent = 50,
                    PricingTiers = new List<PricingTier> { new PricingTier { PricePerUnit = 60m } }
                }
            }
        };

        // On simule que la base de données contient 15 éléments au total
        _mockSearchRepository
            .Setup(repo => repo.GetProductsAsync(It.IsAny<string>(), It.IsAny<List<int>>(), It.IsAny<decimal?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((new List<Product> { fakeProduct }, 15));

        // ACT
        // On demande la page 1 avec une taille de 10
        var result = await _searchService.GetProductsAsync(null, null, null, true, "asc", page: 1, pageSize: 10, "fr-FR");

        // ASSERT
        // 1. Vérification de la pagination complexe (15 éléments / 10 par page = 2 pages)
        Assert.Equal(15, result.Total);
        Assert.Equal(2, result.TotalPages);

        // 2. Vérification du Mapping
        Assert.Single(result.Items);
        var dto = result.Items.First();

        Assert.Equal(100, dto.Id);
        Assert.Equal("Produit Français", dto.Name); // A bien pris la traduction et non le Slug
        Assert.Equal("http://image.png", dto.ImageUrl);
        Assert.Equal("Available", dto.Status);

        // 3. Vérification de la logique de prix : Le service a dû trouver que 30€ (60 - 50%) est le moins cher !
        Assert.Equal(30m, dto.Price);
    }
}