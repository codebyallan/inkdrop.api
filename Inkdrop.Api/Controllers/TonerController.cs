using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Notifications;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TonerController(TonerService tonerService, NotificationContext notificationContext) : ControllerBase
{
    [HttpPost]
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
        TonerResponse? toner = await tonerService.CreateTonerAsync(request);
        if (toner == null) return BadRequest();
        return CreatedAtAction(nameof(GetTonerById), new { id = toner.Id }, toner);
    }
    [HttpGet]
    [EndpointName("GetAllToners")]
    [EndpointSummary("Get all toners")]
    [EndpointDescription("Returns a list of all toners.")]
    [ProducesResponseType(typeof(IEnumerable<TonerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TonerResponse>>> GetToners()
    {
        IEnumerable<TonerResponse> toners = await tonerService.GetAllTonersAsync();
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
        TonerResponse? toner = await tonerService.GetTonerByIdAsync(id);
        if (toner == null) return NotFound();
        return Ok(toner);
    }
    [HttpPut("{id}")]
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
        TonerResponse? updated = await tonerService.UpdateTonerAsync(id, request);
        if (updated == null) return NotFound();
        return Ok(updated);
    }
    [HttpDelete("{id}")]
    [EndpointName("DeleteToner")]
    [EndpointSummary("Delete a toner")]
    [EndpointDescription("Deletes a toner with the specified ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> DeleteToner(Guid id)
    {
        bool deleted = await tonerService.DeleteTonerAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}