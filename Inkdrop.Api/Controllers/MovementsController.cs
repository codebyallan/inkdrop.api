using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovementsController(MovementsService movementsService) : ControllerBase
{
    [HttpPost]
    [EndpointName("CreateMovements")]
    [EndpointSummary("Create a new movement")]
    [EndpointDescription("Creates a new movement with the provided details and returns the created movement.")]
    [ProducesResponseType(typeof(MovementsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovementsResponse>> CreateMovements([FromBody] CreateMovementRequest request)
    {
        MovementsResponse? movement = await movementsService.CreateAsync(request);
        if (movement == null) return BadRequest();
        return CreatedAtAction(nameof(GetMovementsById), new { id = movement.Id }, movement);
    }

    [HttpGet]
    [EndpointName("GetAllMovements")]
    [EndpointSummary("Get all movements")]
    [EndpointDescription("Returns a list of all movements.")]
    [ProducesResponseType(typeof(IEnumerable<MovementsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovementsResponse>>> GetMovements()
    {
        IEnumerable<MovementsResponse> movements = await movementsService.GetAllAsync();
        return Ok(movements);
    }

    [HttpGet("{id}")]
    [EndpointName("GetMovementById")]
    [EndpointSummary("Get a movement by ID")]
    [EndpointDescription("Returns a movement with the specified ID.")]
    [ProducesResponseType(typeof(MovementsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovementsResponse>> GetMovementsById([FromRoute] Guid id)
    {
        MovementsResponse? movement = await movementsService.GetByIdAsync(id);
        if (movement == null) return NotFound();
        return Ok(movement);
    }
}