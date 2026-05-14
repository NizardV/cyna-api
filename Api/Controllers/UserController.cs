using Microsoft.AspNetCore.Mvc;
using Application.Dtos;
using Application.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{

    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.Login(request);

        if (!result.Success || string.IsNullOrEmpty(result.Token))
        {
            return Unauthorized(new { error = result.ErrorMessage });
        }

        return Ok(new { token = result.Token });
    }
}
