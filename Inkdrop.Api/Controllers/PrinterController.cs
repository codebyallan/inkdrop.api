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
    public async Task<ActionResult<PrinterResponse>> CreatePrinter(CreatePrinterRequest request)
    {
        PrinterResponse createdPrinter = await printerService.CreatePrinterAsync(request);
        return CreatedAtAction(nameof(GetPrinterById), new { id = createdPrinter.Id }, createdPrinter);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrinterResponse>>> GetPrinters()
    {
        IEnumerable<PrinterResponse> printers = await printerService.GetPrintersAsync();
        return Ok(printers);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<PrinterResponse>> GetPrinterById(Guid id)
    {
        PrinterResponse? printer = await printerService.GetPrinterByIdAsync(id);
        if (printer == null) return NotFound();
        return Ok(printer);
    }
    [HttpPut("{id}")]
    public async Task<ActionResult<PrinterResponse>> UpdatePrinter(Guid id, UpdatePrinterRequest request)
    {
        PrinterResponse? updatedPrinter = await printerService.UpdatePrinterAsync(id, request);
        if (updatedPrinter == null) return NotFound();
        return Ok(updatedPrinter);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePrinter(Guid id)
    {
        bool deleted = await printerService.DeletePrinterAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}