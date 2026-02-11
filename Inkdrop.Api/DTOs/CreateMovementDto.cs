namespace Inkdrop.Api.DTOs;

public record CreateMovementDto(Guid TonerId, Guid? PrinterId, int Quantity, string? Description, string Type);
