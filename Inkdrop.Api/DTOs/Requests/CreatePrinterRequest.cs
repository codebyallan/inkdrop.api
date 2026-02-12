namespace Inkdrop.Api.DTOs.Requests;

public record CreatePrinterRequest(string Name, string Model, string Manufacturer, string IpAddress, Guid LocationId);