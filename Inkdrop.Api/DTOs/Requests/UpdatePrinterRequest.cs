namespace Inkdrop.Api.DTOs.Requests;

public record UpdatePrinterRequest(string? Name, string? Model, string? Manufacturer, string? IpAddress, bool? IsActive, Guid? LocationId);