using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrinterController(PrinterService printerService) : ControllerBase
{
    [HttpPost]
    [EndpointName("CreatePrinter")]
    [EndpointSummary("Create a new printer")]
    [EndpointDescription("Creates a new printer with the provided details and returns the created printer.")]
    [ProducesResponseType(typeof(PrinterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrinterResponse>> CreatePrinter(CreatePrinterRequest request)
    {
        PrinterResponse? createdPrinter = await printerService.CreatePrinterAsync(request);
        if (createdPrinter == null) return BadRequest();
        return CreatedAtAction(nameof(GetPrinterById), new { id = createdPrinter.Id }, createdPrinter);
    }
    [HttpGet]
    [EndpointName("GetAllPrinters")]
    [EndpointSummary("Get all printers")]
    [EndpointDescription("Returns a list of all printers.")]
    [ProducesResponseType(typeof(IEnumerable<PrinterResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrinterResponse>>> GetPrinters()
    {
        IEnumerable<PrinterResponse> printers = await printerService.GetPrintersAsync();
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
        PrinterResponse? printer = await printerService.GetPrinterByIdAsync(id);
        if (printer == null) return NotFound();
        return Ok(printer);
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
        PrinterResponse? updatedPrinter = await printerService.UpdatePrinterAsync(id, request);
        if (updatedPrinter == null) return NotFound();
        return Ok(updatedPrinter);
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
        bool deleted = await printerService.DeletePrinterAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}