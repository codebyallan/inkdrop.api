using Inkdrop.Api.DTOs;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Technician")]
public sealed class LocationController(ILocationService locationService, NotificationContext notificationContext) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointName("CreateLocation")]
    [EndpointSummary("Create a new location")]
    [EndpointDescription("Creates a new location with the provided details and returns the created location.")]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LocationResponse>> CreateLocation([FromBody] CreateLocationRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError.", "body cannot be empty.");
            return BadRequest();
        }
        var result = await locationService.CreateLocationAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess) return BadRequest();
        return CreatedAtAction(nameof(GetLocationById), new { id = result.Value!.Id }, result.Value);
    }
    [HttpGet]
    [EndpointName("GetLocations")]
    [EndpointSummary("Get all locations")]
    [EndpointDescription("Returns a list of all locations.")]
    [ProducesResponseType(typeof(IEnumerable<LocationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LocationResponse>>> GetLocations()
    {
        IEnumerable<LocationResponse> locations = await locationService.GetAllLocationsAsync(HttpContext.RequestAborted);
        return Ok(locations);
    }
    [HttpGet("{id}")]
    [EndpointName("GetLocationById")]
    [EndpointSummary("Get a location by ID")]
    [EndpointDescription("Returns a location with the specified ID.")]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LocationResponse>> GetLocationById([FromRoute] Guid id)
    {
        var result = await locationService.GetLocationByIdAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        return Ok(result.Value);
    }
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("UpdateLocation")]
    [EndpointSummary("Update an existing location")]
    [EndpointDescription("Updates an existing location with the provided details and returns the updated location.")]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LocationResponse>> UpdateLocation([FromRoute] Guid id, [FromBody] UpdateLocationRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError.", "body cannot be empty.");
            return BadRequest();
        }
        var result = await locationService.UpdateLocationAsync(id, request, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return Ok(result.Value);
    }
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("DeleteLocation")]
    [EndpointSummary("Delete a location by ID")]
    [EndpointDescription("Deletes a location with the specified ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteLocation([FromRoute] Guid id)
    {
        var result = await locationService.DeleteLocationAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return NoContent();
    }
}