using Inkdrop.Api.DTOs;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController(LocationService locationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<LocationResponse>> CreateLocation([FromBody] CreateLocationRequest request)
    {
        LocationResponse? createdLocation = await locationService.CreateLocationAsync(request);
        if (createdLocation == null) return BadRequest();
        return CreatedAtAction(nameof(GetLocationById), new { id = createdLocation.Id }, createdLocation);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationResponse>>> GetLocations()
    {
        IEnumerable<LocationResponse> locations = await locationService.GetAllLocationsAsync();
        return Ok(locations);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<LocationResponse>> GetLocationById([FromRoute] Guid id)
    {
        LocationResponse? location = await locationService.GetLocationByIdAsync(id);
        if (location == null) return NotFound();
        return Ok(location);
    }
    [HttpPut("{id}")]
    public async Task<ActionResult<LocationResponse>> UpdateLocation([FromRoute] Guid id, [FromBody] UpdateLocationRequest request)
    {
        LocationResponse? updatedLocation = await locationService.UpdateLocationAsync(id, request);
        if (updatedLocation == null) return NotFound();
        return Ok(updatedLocation);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation([FromRoute] Guid id)
    {
        bool deleted = await locationService.DeleteLocationAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}