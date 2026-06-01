using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using System.Security.Claims;

namespace Inkdrop.Api.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
[EnableRateLimiting("general-policy")]
public sealed class UserController(IUserService userService, NotificationContext notificationContext) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [EndpointName("GetAllUsers")]
    [EndpointSummary("Get all users")]
    [EndpointDescription("Returns a list of all registered users in the system.")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        var users = await userService.GetAllUsersAsync(HttpContext.RequestAborted);
        return Ok(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    [EndpointName("GetUserById")]
    [EndpointSummary("Get user by ID")]
    [EndpointDescription("Returns the details of a specific user identified by their unique ID.")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var result = await userService.GetUserByIdAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [EndpointName("CreateUser")]
    [EndpointSummary("Register a new user")]
    [EndpointDescription("Creates a new user account with the provided credentials and assigns a role.")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Create([FromBody] RegisterRequest request)
    {
        var result = await userService.CreateUserAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess) return BadRequest(new { Errors = notificationContext.Notifications });
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    [EndpointName("UpdateUser")]
    [EndpointSummary("Update user profile")]
    [EndpointDescription("Updates the username, email, or role of an existing user.")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await userService.UpdateUserAsync(id, request, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { Errors = notificationContext.Notifications });
        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [EndpointName("DeleteUser")]
    [EndpointSummary("Delete a user")]
    [EndpointDescription("Soft-deletes a user account, preventing further logins.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await userService.DeleteUserAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/activate")]
    [EndpointName("ActivateUser")]
    [EndpointSummary("Activate a user account")]
    [EndpointDescription("Sets a deactivated user account back to active status.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await userService.ActivateUserAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return Ok(new { Message = "User activated successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/deactivate")]
    [EndpointName("DeactivateUser")]
    [EndpointSummary("Deactivate a user account")]
    [EndpointDescription("Disables a user account, preventing any further authentication.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await userService.DeactivateUserAsync(id, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest();
        return Ok(new { Message = "User deactivated successfully" });
    }

    [HttpPatch("me/password")]
    [EndpointName("ChangeMyPassword")]
    [EndpointSummary("Change own password")]
    [EndpointDescription("Allows an authenticated user to update their own password after verifying the current one.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError", "body cannot be empty.");
            return BadRequest();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var result = await userService.ChangePasswordAsync(userGuid, request, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { Errors = notificationContext.Notifications });

        return Ok(new { Message = "Password updated successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/password")]
    [EndpointName("ResetUserPassword")]
    [EndpointSummary("Reset user password")]
    [EndpointDescription("Allows an administrator to force a password reset for any user account.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetUserPassword(Guid id, [FromBody] ResetPasswordRequest request)
    {
        if (request is null)
        {
            notificationContext.AddNotification("RequestError", "body cannot be empty.");
            return BadRequest();
        }

        var result = await userService.ResetPasswordAsync(id, request, HttpContext.RequestAborted);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { Errors = notificationContext.Notifications });

        return Ok(new { Message = "Password reset successfully" });
    }
}
