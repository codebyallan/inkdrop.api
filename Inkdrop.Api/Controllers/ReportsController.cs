using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Core;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Notifications;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
[EnableRateLimiting("reports-policy")]
public sealed class ReportsController(IReportsService reportsService) : ControllerBase
{
    private readonly IReportsService _reportsService = reportsService;

    [HttpGet("pages/volume")]
    [EndpointName("GetPageVolume")]
    [EndpointSummary("Get page volume time-series")]
    [EndpointDescription("Returns a time-series of printed page volumes for the fleet or a specific printer. Dates are optional; omitting them returns all available data.")]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPageVolume(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null, 
        [FromQuery] Guid? printerId = null)
    {
        var result = await _reportsService.GetPageVolumeAsync(startDate, endDate, printerId, HttpContext.RequestAborted);
        return result.IsSuccess ? Ok(result.Value) : BadRequest();
    }

    [HttpGet("pages/printed")]
    [EndpointName("GetPrintedPages")]
    [EndpointSummary("Get total printed pages for a period")]
    [EndpointDescription("Calculates and returns the consolidated volume of printed pages for the entire fleet or a specific printer within a given time range. If no dates are provided, the system aggregates all available historical telemetry data up to the current moment.")]
    [ProducesResponseType(typeof(PrintedPagesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPrintedPages(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? printerId = null)
    {
        var result = await _reportsService.GetPrintedPagesAsync(
            startDate, endDate, printerId, HttpContext.RequestAborted);

        return result.IsSuccess ? Ok(result.Value) : BadRequest();
    }

    [HttpGet("toner/consumption")]
    [EndpointName("GetTonerConsumption")]
    [EndpointSummary("Get toner consumption time-series")]
    [EndpointDescription("Analyzes the drop in toner levels over time to calculate actual consumption. Dates are optional.")]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTonerConsumption(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null, 
        [FromQuery] Guid? printerId = null)
    {
        var result = await _reportsService.GetTonerConsumptionAsync(startDate, endDate, printerId, HttpContext.RequestAborted);
        return result.IsSuccess ? Ok(result.Value) : BadRequest();
    }

    [HttpGet("predictions")]
    [EndpointName("GetPredictions")]
    [EndpointSummary("Get toner depletion predictions")]
    [EndpointDescription("Uses linear regression based on telemetry data to predict when toners will be depleted. You can filter by printer or define a specific analysis window via dates.")]
    [ProducesResponseType(typeof(IEnumerable<PredictiveMetricResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPredictions(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null, 
        [FromQuery] Guid? printerId = null)
    {
        var result = await _reportsService.GetPredictiveAnalysisAsync(startDate, endDate, printerId, HttpContext.RequestAborted);
        return result.IsSuccess ? Ok(result.Value) : BadRequest();
    }

    [HttpGet("summary")]
    [EndpointName("GetExecutiveSummary")]
    [EndpointSummary("Get fleet executive summary")]
    [EndpointDescription("Provides a high-level overview of fleet health, total pages, and critical alerts. You can filter by a specific printer to see its individual summary.")]
    [ProducesResponseType(typeof(ExecutiveSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? printerId = null)
    {
        var result = await _reportsService.GetExecutiveSummaryAsync(startDate, endDate, printerId, HttpContext.RequestAborted);
        return result.IsSuccess ? Ok(result.Value) : BadRequest();
    }
}
