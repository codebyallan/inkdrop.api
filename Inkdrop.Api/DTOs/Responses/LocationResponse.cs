namespace Inkdrop.Api.DTOs.Responses;

public record LocationResponse(Guid Id, string Name, string? Description, DateTime CreatedAt);