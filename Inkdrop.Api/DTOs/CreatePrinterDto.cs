namespace Inkdrop.Api.DTOs;

public record CreatePrinterDto(string Name, string Model, string Manufacturer, string IpAddress, Guid LocationId);