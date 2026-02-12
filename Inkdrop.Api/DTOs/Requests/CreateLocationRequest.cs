namespace Inkdrop.Api.DTOs.Requests;

public record CreateLocationRequest(string Name, string? Description = null);