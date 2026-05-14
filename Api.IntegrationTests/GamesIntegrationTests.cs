using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Dtos;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Api.IntegrationTests;

public class TestDto
{
    public string Token { get; set; } = string.Empty;
}

public class GamesIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public GamesIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllGames_ShouldReturn200AndSeededGame()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            await DbInitializer.SeedAsync(db);
        }
        // Act
        var response1 = await _client.GetAsync("/api/games");

        // Assert (HTTP)
        Assert.Equal(HttpStatusCode.Unauthorized, response1.StatusCode);

        var body = new
        {
            Username = "plop",
            Password = "plop"
        };

        HttpContent content = JsonContent.Create(body);

        var appelToken = await _client.PostAsync("api/User/login",content);

        TestDto token = await appelToken.Content.ReadFromJsonAsync<TestDto>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        var response2 = await _client.GetAsync("/api/games");

        // Assert (contenu)
        var games = await response2.Content.ReadFromJsonAsync<List<GameDto>>();
        Assert.NotNull(games);
        Assert.Contains(games!, g => g.Title == "Elden Ring");
    }
}
