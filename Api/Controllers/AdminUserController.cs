namespace Api.Controllers;

using Application.Interfaces;

using Domain.Dto.User;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NLog;

using Tools;

/// <summary>
/// Contrôleur d'administration des utilisateurs.
/// Toutes les routes sont protégées (Admin ou SuperAdmin uniquement).
/// </summary>
[ApiController]
[Route("admin/users")]
[Authorize("AdminOnly")]
[Produces("application/json")]
public class AdminUserController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IUserService _userService;

    public AdminUserController(IUserService userService)
    {
        _userService = userService;
    }

    // ── GET /admin/users ──────────────────────────────────────────────────────

    /// <summary>
    /// Retourne la liste de tous les utilisateurs, à l'exception de l'admin connecté.
    /// </summary>
    /// <response code="200">Liste récupérée.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="403">Droits insuffisants.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers()
    {
        var currentAdminId = ClaimsHelper.GetUserId(User);
        _logger.Info("GET /admin/users — admin ID {AdminId}", currentAdminId);

        var users = await _userService.GetAllUsersExceptAsync(currentAdminId);
        return Ok(users);
    }

    // ── PATCH /admin/users/{id}/disable ──────────────────────────────────────

    /// <summary>
    /// Désactive le compte d'un utilisateur.
    /// Un compte désactivé ne peut plus se connecter ni renouveler son token.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur cible.</param>
    /// <response code="200">Compte désactivé.</response>
    /// <response code="404">Utilisateur introuvable.</response>
    [HttpPatch("{id:int}/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableUser(int id)
    {
        _logger.Info("PATCH /admin/users/{Id}/disable", id);
        try
        {
            await _userService.SetUserDisabledAsync(id, isDisabled: true);
            return Ok(new { message = $"Utilisateur {id} désactivé." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ── PATCH /admin/users/{id}/enable ────────────────────────────────────────

    /// <summary>
    /// Réactive le compte d'un utilisateur précédemment désactivé.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur cible.</param>
    /// <response code="200">Compte réactivé.</response>
    /// <response code="404">Utilisateur introuvable.</response>
    [HttpPatch("{id:int}/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableUser(int id)
    {
        _logger.Info("PATCH /admin/users/{Id}/enable", id);
        try
        {
            await _userService.SetUserDisabledAsync(id, isDisabled: false);
            return Ok(new { message = $"Utilisateur {id} réactivé." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ── PATCH /admin/users/{id}/role ──────────────────────────────────────────

    /// <summary>
    /// Change le rôle d'un utilisateur.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur cible.</param>
    /// <param name="dto">Nouveau rôle à assigner.</param>
    /// <response code="200">Rôle mis à jour.</response>
    /// <response code="400">Rôle invalide.</response>
    /// <response code="404">Utilisateur introuvable.</response>
    [HttpPatch("{id:int}/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Convertir la string en UserRole
        if (!Enum.TryParse<UserRole>(dto.Role, true, out var newRole))
        {
            // Si la conversion échoue, essayer de mapper depuis la description française
            newRole = dto.Role switch
            {
                "Utilisateur" => UserRole.User,
                "Administrateur" => UserRole.Admin,
                "Super administrateur" => UserRole.SuperAdmin,
                _ => throw new ArgumentException($"Invalid role: {dto.Role}")
            };
        }

        _logger.Info("PATCH /admin/users/{Id}/role — Role={Role}", id, dto.Role);

        try
        {
            await _userService.SetUserRoleAsync(id, newRole);
            return Ok(new { message = $"Rôle de l'utilisateur {id} changé en « {newRole.GetEnumDescription()} »." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}