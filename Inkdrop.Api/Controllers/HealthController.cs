using Inkdrop.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [EndpointName("HealthCheck")]
    [EndpointSummary("Health Check")]
    [EndpointDescription("Checks if the API and the database are healthy and responsive. Used for keep-alive services.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            // Performing a lightweight query to the database to ensure connectivity.
            // This prevents Supabase from pausing and Render from sleeping.
            await dbContext.Users.AnyAsync(cancellationToken);
            
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
