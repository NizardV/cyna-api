using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _repository;

    public GameService(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GameDto>> GetAllAsync()
    {
        var games = await _repository.GetAllAsync();
        return games.Select(MapToDto).ToList();
    }

    public async Task<GameDto?> GetByIdAsync(int id)
    {
        var game = await _repository.GetByIdAsync(id);
        return game == null ? null : MapToDto(game);
    }

    public async Task<GameDto> AddAsync(CreateGameDto dto)
    {
        var game = new Game
        {
            Title = dto.Title,
            Platform = dto.Platform,
            Genre = dto.Genre,
            ReleaseDate = dto.ReleaseDate
        };

        var created = await _repository.AddAsync(game);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(int id, UpdateGameDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return false;
        }

        existing.Title = dto.Title;
        existing.Platform = dto.Platform;
        existing.Genre = dto.Genre;
        existing.ReleaseDate = dto.ReleaseDate;

        return await _repository.UpdateAsync(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static GameDto MapToDto(Game game)
    {
        return new GameDto
        {
            Id = game.Id,
            Title = game.Title,
            Platform = game.Platform,
            Genre = game.Genre,
            ReleaseDate = game.ReleaseDate
        };
    }
}
