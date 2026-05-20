namespace Inkdrop.Api.DTOs.Responses;

public record AuthResponse(Guid Id, string Username, string Email, string Role);
