namespace Inkdrop.Api.Dtos.Responses;

public record TonerResponse(Guid Id, string Model, string Manufacturer, string Color, int Quantity, DateTime CreatedAt);