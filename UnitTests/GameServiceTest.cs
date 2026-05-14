using Application.Dtos;
using Application.Services;
using UnitTests.Fakes;

namespace UnitTests;

public class GameServiceTest
{
    [Fact]
    public async Task AddAsync_ShouldCreateGameAndReturnDtoWithId()
    {
        // Arrange
        var repository = new FakeGameRepository();
        var service = new GameService(repository);

        var dto = new CreateGameDto
        {
            Title = "Elden Ring",
            Platform = "PC",
            Genre = "Action-RPG",
            ReleaseDate = new DateTime(2022, 2, 25)
        };

        // Act
        var created = await service.AddAsync(dto);

        // Assert
        Assert.True(created.Id > 0);
        Assert.Equal("Elden Ring", created.Title);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllGames()
    {
        // Arrange
        var repository = new FakeGameRepository();
        var service = new GameService(repository);

        await service.AddAsync(new CreateGameDto { Title = "Game A", Platform = "PC" });
        await service.AddAsync(new CreateGameDto { Title = "Game B", Platform = "PS5" });

        // Act
        var games = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, games.Count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenGameDoesNotExist()
    {
        // Arrange
        var repository = new FakeGameRepository();
        var service = new GameService(repository);

        var update = new UpdateGameDto
        {
            Title = "Updated",
            Platform = "PC",
            Genre = "Genre",
            ReleaseDate = null
        };

        // Act
        var success = await service.UpdateAsync(id: 999, update);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenGameExistAndModified()
    {
        // Arrange
        var repository = new FakeGameRepository();
        var service = new GameService(repository);

        await service.AddAsync(new CreateGameDto { Title = "To Update", Platform = "PC" });

        var update = new UpdateGameDto
        {
            Title = "Updated",
            Platform = "PC",
            Genre = "Genre",
            ReleaseDate = null
        };

        // Act
        var success = await service.UpdateAsync(id: 1, update);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenGameExists()
    {
        // Arrange
        var repository = new FakeGameRepository();
        var service = new GameService(repository);

        var created = await service.AddAsync(new CreateGameDto { Title = "To Delete", Platform = "PC" });

        // Act
        var success = await service.DeleteAsync(created.Id);

        // Assert
        Assert.True(success);
        var all = await service.GetAllAsync();
        Assert.Empty(all);
    }
}
