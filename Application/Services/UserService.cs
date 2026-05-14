using Application.Dtos;
using Application.Interfaces;
using Domain.Repositories;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository, ITokenGenerator tokenGenerator, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResultDto> Login(LoginRequestDto request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                };
            }

            var valid = _passwordHasher.VerifyPassword(user.Password, request.Password);
            if (!valid)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                };
            }

            var token = _tokenGenerator.GenerateToken(user);

            return new LoginResultDto
            {
                Success = true,
                Token = token
            };
        }
    }
}
