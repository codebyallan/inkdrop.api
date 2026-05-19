using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/user")]
[Authorize(Roles = "Admin")]
public sealed class UserController(IUserService userService, NotificationContext notificationContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);
        if (user is null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] RegisterRequest request)
    {
        var user = await userService.CreateUserAsync(request);
        if (user is null) return BadRequest(new { Errors = notificationContext.Notifications });
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await userService.UpdateUserAsync(id, request);
        if (user is null) return NotFound();
        if (!notificationContext.IsValid) return BadRequest(new { Errors = notificationContext.Notifications });
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await userService.DeleteUserAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var success = await userService.ActivateUserAsync(id);
        if (!success) return NotFound();
        return Ok(new { Message = "User activated successfully" });
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success = await userService.DeactivateUserAsync(id);
        if (!success) return NotFound();
        return Ok(new { Message = "User deactivated successfully" });
    }
}
