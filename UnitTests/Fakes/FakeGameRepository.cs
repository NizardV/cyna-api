using Domain.Entities;
using Domain.Repositories;

namespace UnitTests.Fakes;

public class FakeGameRepository : IGameRepository
{
    private readonly List<Game> _games = new();
    private int _nextId = 1;

    public async Task<List<Game>> GetAllAsync() => _games.ToList();

    public async Task<Game?> GetByIdAsync(int id) => _games.FirstOrDefault(g => g.Id == id);

    public async Task<Game> AddAsync(Game game)
    {
        game.Id = _nextId++;
        _games.Add(game);
        return game;
    }

    public async Task<bool> UpdateAsync(Game game)
    {
        var index = _games.FindIndex(g => g.Id == game.Id);
        if (index == -1)
        {
            return false;
        }

        _games[index] = game;
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = _games.FirstOrDefault(g => g.Id == id);
        if (existing == null)
        {
            return false;
        }

        _games.Remove(existing);
        return true;
    }
}
