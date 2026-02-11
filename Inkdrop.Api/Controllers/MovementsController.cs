using Inkdrop.Api.DTOs;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovementsController(MovementsService movementsService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Movements>> CreateMovements([FromBody] CreateMovementDto request)
    {
        Movements movement = await movementsService.CreateAsync(request);
        return CreatedAtAction(nameof(GetMovementsById), new { id = movement.Id }, movement);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movements>>> GetMovements()
    {
        IEnumerable<Movements> movements = await movementsService.GetAllAsync();
        return Ok(movements);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Movements>> GetMovementsById([FromRoute] Guid id)
    {
        Movements? movement = await movementsService.GetByIdAsync(id);
        if (movement == null) return NotFound();
        return Ok(movement);
    }
}