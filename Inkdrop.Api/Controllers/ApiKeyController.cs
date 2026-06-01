using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("general-policy")]
public sealed class ApiKeyController(IApiKeyService apiKeyService, NotificationContext notificationContext) : ControllerBase
{
    [HttpPost]
    [EndpointName("CreateApiKey")]
    [EndpointSummary("Create a new API Key for a Bot")]
    [EndpointDescription("Generates a unique API key for an external agent. The plain key is returned only once.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> Create([FromBody] CreateApiKeyRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError", "Request body cannot be empty.");
            return BadRequest();
        }

        var result = await apiKeyService.CreateKeyAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess) return BadRequest();

        return CreatedAtAction(nameof(GetKeys), result.Value);
    }

    [HttpGet]
    [EndpointName("GetApiKeys")]
    [EndpointSummary("List all active API Keys")]
    [EndpointDescription("Returns a list of all currently active API keys and their last usage date.")]
    [ProducesResponseType(typeof(IEnumerable<ApiKeyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApiKeyResponse>>> GetKeys()
    {
        var keys = await apiKeyService.GetActiveKeysAsync(HttpContext.RequestAborted);
        return Ok(keys);
    }

    [HttpPut("{id}")]
    [EndpointName("UpdateApiKeyName")]
    [EndpointSummary("Update API Key Name")]
    [EndpointDescription("Updates the descriptive name of an existing API key.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApiKeyRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError", "Request body cannot be empty.");
            return BadRequest();
        }

        var result = await apiKeyService.UpdateKeyNameAsync(id, request);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();

        return NoContent();
    }

    [HttpDelete("{id}")]

    [EndpointName("RevokeApiKey")]
    [EndpointSummary("Revoke an API Key")]
    [EndpointDescription("Deactivates an API key, preventing any further access for the associated agent.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke(Guid id)
    {
        var result = await apiKeyService.RevokeKeyAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();

        return NoContent();
    }
}
