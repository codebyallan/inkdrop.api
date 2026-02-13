using Inkdrop.Api.DTOs;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Notifications;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController(LocationService locationService, NotificationContext notificationContext) : ControllerBase
{
    [HttpPost]
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
        LocationResponse? createdLocation = await locationService.CreateLocationAsync(request);
        if (createdLocation == null) return BadRequest();
        return CreatedAtAction(nameof(GetLocationById), new { id = createdLocation.Id }, createdLocation);
    }
    [HttpGet]
    [EndpointName("GetLocations")]
    [EndpointSummary("Get all locations")]
    [EndpointDescription("Returns a list of all locations.")]
    [ProducesResponseType(typeof(IEnumerable<LocationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LocationResponse>>> GetLocations()
    {
        IEnumerable<LocationResponse> locations = await locationService.GetAllLocationsAsync();
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
        LocationResponse? location = await locationService.GetLocationByIdAsync(id);
        if (location == null) return NotFound();
        return Ok(location);
    }
    [HttpPut("{id}")]
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
        LocationResponse? updatedLocation = await locationService.UpdateLocationAsync(id, request);
        if (updatedLocation == null) return NotFound();
        return Ok(updatedLocation);
    }
    [HttpDelete("{id}")]
    [EndpointName("DeleteLocation")]
    [EndpointSummary("Delete a location by ID")]
    [EndpointDescription("Deletes a location with the specified ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteLocation([FromRoute] Guid id)
    {
        bool deleted = await locationService.DeleteLocationAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}