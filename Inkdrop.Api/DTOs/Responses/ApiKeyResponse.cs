namespace Inkdrop.Api.DTOs.Responses;

public record ApiKeyResponse(Guid Id, string Name, DateTime CreatedAt, DateTime? LastUsedAt);
