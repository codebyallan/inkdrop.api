using Inkdrop.Api.DTOs;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TonerController(TonerService tonerService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateToner([FromBody] CreateTonerDto request)
    {
        Toner toner = await tonerService.CreateTonerAsync(request);
        return CreatedAtAction(nameof(GetTonerById), new { id = toner.Id }, toner);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Toner>>> GetToners()
    {
        IEnumerable<Toner> toners = await tonerService.GetAllTonersAsync();
        return Ok(toners);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Toner>> GetTonerById(Guid id)
    {
        Toner? toner = await tonerService.GetTonerByIdAsync(id);
        if (toner == null) return NotFound();
        return Ok(toner);
    }
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateToner(Guid id, [FromBody] UpdateTonerDto request)
    {
        Toner? updated = await tonerService.UpdateTonerAsync(id, request);
        if (updated == null) return NotFound();
        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteToner(Guid id)
    {
        bool deleted = await tonerService.DeleteTonerAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}