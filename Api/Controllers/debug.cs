namespace Api.Controllers;

using Domain.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


public class DebugController : ControllerBase
{
    [HttpGet("debug-claims")]
    [AllowAnonymous]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }
}