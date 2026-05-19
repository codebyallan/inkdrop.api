namespace Inkdrop.Api.DTOs.Responses;

public record UserResponse(Guid Id, string Username, string Email, string Role, bool IsActive, DateTime CreatedAt);
