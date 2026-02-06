namespace Inkdrop.Api.DTOs;

public record UpdatePrinterDto(string? Name, string? Model, string? Manufacturer, string? IpAddress, bool? IsActive, Guid? LocationId);