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
        var result = await userService.GetUserByIdAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] RegisterRequest request)
    {
        var result = await userService.CreateUserAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess) return BadRequest(new { Errors = notificationContext.Notifications });
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await userService.UpdateUserAsync(id, request, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { Errors = notificationContext.Notifications });
        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await userService.DeleteUserAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return NoContent();
    }

    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await userService.ActivateUserAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return Ok(new { Message = "User activated successfully" });
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await userService.DeactivateUserAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
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

        var result = await userService.ChangePasswordAsync(id, request, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { Errors = notificationContext.Notifications });

        return Ok(new { Message = "Password updated successfully" });
    }
}
