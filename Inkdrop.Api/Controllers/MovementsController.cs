using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovementsController(MovementsService movementsService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MovementsResponse>> CreateMovements([FromBody] CreateMovementRequest request)
    {
        MovementsResponse? movement = await movementsService.CreateAsync(request);
        if (movement == null) return BadRequest();
        return CreatedAtAction(nameof(GetMovementsById), new { id = movement.Id }, movement);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovementsResponse>>> GetMovements()
    {
        IEnumerable<MovementsResponse> movements = await movementsService.GetAllAsync();
        return Ok(movements);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovementsResponse>> GetMovementsById([FromRoute] Guid id)
    {
        MovementsResponse? movement = await movementsService.GetByIdAsync(id);
        if (movement == null) return NotFound();
        return Ok(movement);
    }
}