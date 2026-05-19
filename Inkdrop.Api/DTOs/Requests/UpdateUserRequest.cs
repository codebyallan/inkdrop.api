using Inkdrop.Api.Entities;

namespace Inkdrop.Api.DTOs.Requests;

public record UpdateUserRequest(string? Username, string? Email, UserRole? Role);
