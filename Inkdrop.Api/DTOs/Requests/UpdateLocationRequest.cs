namespace Inkdrop.Api.DTOs.Requests;

public record UpdateLocationRequest(string? Name, string? Description = null);