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
public sealed class TonerController(ITonerService tonerService, NotificationContext notificationContext) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointName("CreateToner")]
    [EndpointSummary("Create a new toner")]
    [EndpointDescription("Creates a new toner with the provided details and returns the created toner.")]
    [ProducesResponseType(typeof(TonerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TonerResponse>> CreateToner([FromBody] CreateTonerRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError.", "body cannot be empty.");
            return BadRequest();
        }
        var result = await tonerService.CreateTonerAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess) return BadRequest();
        return CreatedAtAction(nameof(GetTonerById), new { id = result.Value!.Id }, result.Value);
    }
    [HttpGet]
    [EndpointName("GetAllToners")]
    [EndpointSummary("Get all toners")]
    [EndpointDescription("Returns a list of all toners.")]
    [ProducesResponseType(typeof(IEnumerable<TonerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TonerResponse>>> GetToners()
    {
        IEnumerable<TonerResponse> toners = await tonerService.GetAllTonersAsync(HttpContext.RequestAborted);
        return Ok(toners);
    }
    [HttpGet("{id}")]
    [EndpointName("GetTonerById")]
    [EndpointSummary("Get a toner by ID")]
    [EndpointDescription("Returns a toner with the specified ID.")]
    [ProducesResponseType(typeof(TonerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TonerResponse>> GetTonerById(Guid id)
    {
        var result = await tonerService.GetTonerByIdAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        return Ok(result.Value);
    }
    [HttpGet("low")]
    [EndpointName("GetLowStock")]
    [EndpointSummary("Get toners with low stock levels")]
    [EndpointDescription("Returns a list of all toners where the current quantity is below the specified threshold. If no threshold is provided, it defaults to 3.")]
    [ProducesResponseType(typeof(IEnumerable<TonerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TonerResponse>>> GetLowStock([FromQuery] int threshold = 3)
    {
        IEnumerable<TonerResponse> toners = await tonerService.GetLowerTonersAsync(threshold, HttpContext.RequestAborted);
        return Ok(toners);
    }
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("UpdateToner")]
    [EndpointSummary("Update an existing toner")]
    [EndpointDescription("Updates an existing toner with the provided details and returns the updated toner.")]
    [ProducesResponseType(typeof(TonerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TonerResponse>> UpdateToner(Guid id, [FromBody] UpdateTonerRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError.", "body cannot be empty.");
            return BadRequest();
        }
        var result = await tonerService.UpdateTonerAsync(id, request, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return Ok(result.Value);
    }
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("DeleteToner")]
    [EndpointSummary("Delete a toner")]
    [EndpointDescription("Deletes a toner with the specified ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteToner(Guid id)
    {
        var result = await tonerService.DeleteTonerAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return NoContent();
    }
}