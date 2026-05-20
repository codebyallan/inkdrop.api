using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public sealed class PrinterController(IPrinterService printerService, NotificationContext notificationContext) : ControllerBase
{
    [HttpPost]
    [EndpointName("CreatePrinter")]
    [EndpointSummary("Create a new printer")]
    [EndpointDescription("Creates a new printer with the provided details and returns the created printer.")]
    [ProducesResponseType(typeof(PrinterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrinterResponse>> CreatePrinter([FromBody] CreatePrinterRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError.", "body cannot be empty.");
            return BadRequest();
        }
        var result = await printerService.CreatePrinterAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess) return BadRequest();
        return CreatedAtAction(nameof(GetPrinterById), new { id = result.Value!.Id }, result.Value);
    }
    [HttpGet]
    [EndpointName("GetAllPrinters")]
    [EndpointSummary("Get all printers")]
    [EndpointDescription("Returns a list of all printers.")]
    [ProducesResponseType(typeof(IEnumerable<PrinterResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrinterResponse>>> GetPrinters()
    {
        IEnumerable<PrinterResponse> printers = await printerService.GetAllPrintersAsync(HttpContext.RequestAborted);
        return Ok(printers);
    }
    [HttpGet("{id}")]
    [EndpointName("GetPrinterById")]
    [EndpointSummary("Get a printer by ID")]
    [EndpointDescription("Returns a printer with the specified ID.")]
    [ProducesResponseType(typeof(PrinterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrinterResponse>> GetPrinterById(Guid id)
    {
        var result = await printerService.GetPrinterByIdAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        return Ok(result.Value);
    }
    [HttpPut("{id}")]
    [EndpointName("UpdatePrinter")]
    [EndpointSummary("Update an existing printer")]
    [EndpointDescription("Updates an existing printer with the provided details and returns the updated printer.")]
    [ProducesResponseType(typeof(PrinterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrinterResponse>> UpdatePrinter(Guid id, UpdatePrinterRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError.", "body cannot be empty.");
            return BadRequest();
        }
        var result = await printerService.UpdatePrinterAsync(id, request, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return Ok(result.Value);
    }
    [HttpDelete("{id}")]
    [EndpointName("DeletePrinter")]
    [EndpointSummary("Delete a printer")]
    [EndpointDescription("Deletes a printer with the specified ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePrinter(Guid id)
    {
        var result = await printerService.DeletePrinterAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return NoContent();
    }
}