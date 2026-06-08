namespace Application.Interfaces.Services;

using Domain.Dto.Cart;

public interface ICartService
{
    Task<CartResultDto> AddOrUpdateCartItemAsync(int userId, AddCartItemRequestDto dto);
}
