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
        var users = await userService.GetAllUsersAsync(HttpContext.RequestAborted);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id, HttpContext.RequestAborted);
        if (user is null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] RegisterRequest request)
    {
        var user = await userService.CreateUserAsync(request, HttpContext.RequestAborted);
        if (user is null) return BadRequest(new { Errors = notificationContext.Notifications });
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await userService.UpdateUserAsync(id, request, HttpContext.RequestAborted);
        if (user is null) return NotFound();
        if (!notificationContext.IsValid) return BadRequest(new { Errors = notificationContext.Notifications });
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await userService.DeleteUserAsync(id, HttpContext.RequestAborted);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var success = await userService.ActivateUserAsync(id, HttpContext.RequestAborted);
        if (!success) return NotFound();
        return Ok(new { Message = "User activated successfully" });
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success = await userService.DeactivateUserAsync(id, HttpContext.RequestAborted);
        if (!success) return NotFound();
        return Ok(new { Message = "User deactivated successfully" });
    }

    [HttpPatch("{id}/password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError", "body cannot be empty.");
            return BadRequest();
        }

        var success = await userService.ChangePasswordAsync(id, request, HttpContext.RequestAborted);
        if (!success && notificationContext.IsValid) return NotFound();
        if (!success) return BadRequest(new { Errors = notificationContext.Notifications });

        return Ok(new { Message = "Password updated successfully" });
    }
}
