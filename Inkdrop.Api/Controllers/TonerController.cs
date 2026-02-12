using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TonerController(TonerService tonerService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TonerResponse>> CreateToner([FromBody] CreateTonerRequest request)
    {
        TonerResponse toner = await tonerService.CreateTonerAsync(request);
        return CreatedAtAction(nameof(GetTonerById), new { id = toner.Id }, toner);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TonerResponse>>> GetToners()
    {
        IEnumerable<TonerResponse> toners = await tonerService.GetAllTonersAsync();
        return Ok(toners);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<TonerResponse>> GetTonerById(Guid id)
    {
        TonerResponse? toner = await tonerService.GetTonerByIdAsync(id);
        if (toner == null) return NotFound();
        return Ok(toner);
    }
    [HttpPut("{id}")]
    public async Task<ActionResult<TonerResponse>> UpdateToner(Guid id, [FromBody] UpdateTonerRequest request)
    {
        TonerResponse? updated = await tonerService.UpdateTonerAsync(id, request);
        if (updated == null) return NotFound();
        return Ok(updated);
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteToner(Guid id)
    {
        bool deleted = await tonerService.DeleteTonerAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}