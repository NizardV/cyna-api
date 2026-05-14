using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class EfGameRepository : IGameRepository
{
    private readonly AppDbContext _context;

    public EfGameRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Game>> GetAllAsync()
    {
        return await _context.Games
            .AsNoTracking()
            .OrderBy(g => g.Id)
            .ToListAsync();
    }

    public async Task<Game?> GetByIdAsync(int id)
    {
        return await _context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Game> AddAsync(Game game)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }

    public async Task<bool> UpdateAsync(Game game)
    {
        var exists = await _context.Games.AnyAsync(g => g.Id == game.Id);
        if (!exists)
        {
            return false;
        }

        _context.Games.Update(game);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);
        if (entity == null)
        {
            return false;
        }

        _context.Games.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
