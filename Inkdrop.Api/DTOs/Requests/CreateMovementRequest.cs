namespace Inkdrop.Api.DTOs.Requests;

public record CreateMovementRequest(Guid TonerId, Guid? PrinterId, int Quantity, string? Description, string Type);
