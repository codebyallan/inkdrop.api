namespace Inkdrop.Api.Dtos.Responses;

public record MovementsResponse(Guid Id, Guid TonerId, Guid? PrinterId, int Quantity, string? Description, string Type, DateTime CreatedAt);