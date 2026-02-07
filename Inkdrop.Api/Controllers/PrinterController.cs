using Inkdrop.Api.DTOs;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrinterController(PrinterService printerService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Printer>> CreatePrinter(CreatePrinterDto request)
    {
        Printer createdPrinter = await printerService.CreatePrinterAsync(request);
        return CreatedAtAction(nameof(GetPrinterById), new { id = createdPrinter.Id }, createdPrinter);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Printer>>> GetPrinters()
    {
        IEnumerable<Printer> printers = await printerService.GetPrintersAsync();
        return Ok(printers);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Printer>> GetPrinterById(Guid id)
    {
        Printer? printer = await printerService.GetPrinterByIdAsync(id);
        if (printer == null) return NotFound();
        return Ok(printer);
    }
    [HttpPut("{id}")]
    public async Task<ActionResult<Printer>> UpdatePrinter(Guid id, UpdatePrinterDto request)
    {
        Printer? updatedPrinter = await printerService.UpdatePrinterAsync(id, request);
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