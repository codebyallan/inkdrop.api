using Inkdrop.Api.Entities;

namespace Inkdrop.Api.DTOs.Requests;

public record RegisterRequest(string Username, string Email, string Password, UserRole Role);
