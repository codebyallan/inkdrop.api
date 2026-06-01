using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/bot")]
[Authorize(Policy = "BotPolicy")]
[EnableRateLimiting("bot-policy")]
public sealed class BotController(IBotService botService, NotificationContext notificationContext) : ControllerBase
{
    [HttpGet("printers")]
    [EndpointName("GetBotPrinters")]
    [EndpointSummary("Get printers for agent monitoring")]
    [EndpointDescription("Returns a list of active printers including their IP and location for the monitoring agent.")]
    [ProducesResponseType(typeof(IEnumerable<BotPrinterResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BotPrinterResponse>>> GetPrinters()
    {
        var printers = await botService.GetMonitoredPrintersAsync(HttpContext.RequestAborted);
        return Ok(printers);
    }

    [HttpPost("telemetry")]
    [EndpointName("PostTelemetry")]
    [EndpointSummary("Report printer telemetry")]
    [EndpointDescription("Receives telemetry data (page count and toner levels) collected by the agent via SNMP.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReportTelemetry([FromBody] TelemetryRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError", "Request body cannot be empty.");
            return BadRequest();
        }

        var result = await botService.SaveTelemetryAsync(request, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        
        if (!result.IsSuccess)
        {
            if (notificationContext.HasNotification("TelemetryDuplicate"))
            {
                return Ok(); // Idempotent response: data already exists
            }
            return BadRequest();
        }

        return CreatedAtAction(nameof(GetPrinters), null, null);
    }
}
