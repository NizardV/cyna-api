using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Dtos;
using Application.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    [EndpointSummary("Retrieve all games")]
    [EndpointDescription("Allows you to retrieve the raw list of all games")]
    [ProducesResponseType(typeof(List<GameDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GameDto>>> GetAll()
    {
        var games = await _gameService.GetAllAsync();
        return Ok(games);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Get all game information")]
    [EndpointDescription("Allows you to retrieve all information related to a game (Title, Platform etc.)")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameDto>> GetById(int id)
    {
        var game = await _gameService.GetByIdAsync(id);
        if (game == null)
        {
            return NotFound();
        }

        return Ok(game);
    }

    [HttpPost]
    [EndpointSummary("Create a game")]
    [EndpointDescription("Allows you to create a game")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest, Description = "Fields is missing or invalid")]
    public async Task<ActionResult<GameDto>> Create([FromBody] CreateGameDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _gameService.AddAsync(dto);

        // Retourne 201 Created avec l’URL du nouvel objet
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Update game information")]
    [EndpointDescription("Allows you to update information related to a game (Title, Platform etc.)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest, Description = "Fields is missing or invalid")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGameDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _gameService.UpdateAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }

        return NoContent(); // 204
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Delete a game")]
    [EndpointDescription("Allows you to delete a game")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _gameService.DeleteAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
