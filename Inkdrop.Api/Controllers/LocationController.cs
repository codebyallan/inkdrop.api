using Inkdrop.Api.DTOs;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController(LocationService locationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Location>> CreateLocation([FromBody] CreateLocationDto request)
    {
        Location createdLocation = await locationService.CreateLocationAsync(request);
        return CreatedAtAction(nameof(GetLocationById), new { id = createdLocation.Id }, createdLocation);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
    {
        IEnumerable<Location> locations = await locationService.GetAllLocationsAsync();
        return Ok(locations);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Location>> GetLocationById([FromRoute] Guid id)
    {
        Location? location = await locationService.GetLocationByIdAsync(id);
        if (location == null) return NotFound();
        return Ok(location);
    }
    [HttpPut("{id}")]
    public async Task<ActionResult<Location>> UpdateLocation([FromRoute] Guid id, [FromBody] UpdateLocationDto request)
    {
        Location? updatedLocation = await locationService.UpdateLocationAsync(id, request);
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