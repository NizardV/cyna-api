namespace Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NLog;

using ILogger = NLog.ILogger;

public class DebugController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    [HttpGet("debug-claims")]
    [AllowAnonymous]
    public IActionResult DebugClaims()
    {
        // log token
        _logger.Info("DebugClaims: User.Identity.Name = {Name}, IsAuthenticated = {IsAuthenticated}", User.Identity?.Name, User.Identity?.IsAuthenticated);
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        _logger.Info("DebugClaims: Claims = {@Claims}", claims);

        return Ok(claims);
    }
}