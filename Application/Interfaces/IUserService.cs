using Application.Dtos;

namespace Application.Interfaces;

public interface IUserService
{
    Task<LoginResultDto> Login(LoginRequestDto loginRequestDto);
}
