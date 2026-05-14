using Application.Dtos;

namespace Application.Interfaces;

public interface IGameService
{
    Task<List<GameDto>> GetAllAsync();
    Task<GameDto?> GetByIdAsync(int id);
    Task<GameDto> AddAsync(CreateGameDto dto);
    Task<bool> UpdateAsync(int id, UpdateGameDto dto);
    Task<bool> DeleteAsync(int id);
}
