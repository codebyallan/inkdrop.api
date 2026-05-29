using Inkdrop.Api.Core;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Technician")]
public sealed class MovementsController(IMovementsService movementsService, NotificationContext notificationContext) : ControllerBase
{
    [HttpPost]
    [EndpointName("CreateMovements")]
    [EndpointSummary("Create a new movement")]
    [EndpointDescription("Creates a new movement with the provided details and returns the created movement.")]
    [ProducesResponseType(typeof(MovementsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovementsResponse>> CreateMovements([FromBody] CreateMovementRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError.", "body cannot be empty.");
            return BadRequest();
        }
        var result = await movementsService.CreateAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess) return BadRequest();
        return CreatedAtAction(nameof(GetMovementsById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet]
    [EndpointName("GetAllMovements")]
    [EndpointSummary("Get all movements")]
    [EndpointDescription("Returns a list of all movements.")]
    [ProducesResponseType(typeof(IEnumerable<MovementsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovementsResponse>>> GetMovements()
    {
        IEnumerable<MovementsResponse> movements = await movementsService.GetAllMovementsAsync(HttpContext.RequestAborted);
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
        var result = await movementsService.GetMovementByIdAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return Ok(result.Value);
    }
    [HttpGet("printer/{id}")]
    [EndpointName("GetMovementsByPrinterId")]
    [EndpointSummary("Get all movements for a specific printer")]
    [EndpointDescription("Returns a chronological list of stock movements OUT associated with the specified Printer ID.")]
    [ProducesResponseType(typeof(IEnumerable<MovementsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovementsResponse>>> GetMovementsByPrinterId([FromRoute] Guid id)
    {
        IEnumerable<MovementsResponse> movements = await movementsService.GetMovementsByPrinterIdAsync(id, HttpContext.RequestAborted);
        return Ok(movements);
    }
    [HttpGet("toner/{id}")]
    [EndpointName("GetMovementsByTonerId")]
    [EndpointSummary("Get all movements for a specific toner")]
    [EndpointDescription("Returns a chronological list of all IN and OUT movements associated with the specified Toner ID.")]
    [ProducesResponseType(typeof(IEnumerable<MovementsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovementsResponse>>> GetMovementsByTonerId([FromRoute] Guid id)
    {
        IEnumerable<MovementsResponse> movements = await movementsService.GetMovementsByTonerIdAsync(id, HttpContext.RequestAborted);
        return Ok(movements);
    }
}