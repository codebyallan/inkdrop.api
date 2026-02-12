namespace Inkdrop.Api.DTOs.Responses;

public record PrinterResponse(Guid Id, string Name, string Model, string Manufacturer, string IpAddress, bool IsActive, Guid LocationId, DateTime CreatedAt);