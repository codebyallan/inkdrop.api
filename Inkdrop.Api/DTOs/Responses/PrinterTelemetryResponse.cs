using System.Collections.Generic;

namespace Inkdrop.Api.DTOs.Responses;

public record PrinterTelemetryResponse(
    int TotalPages,
    string Status,
    List<TonerTelemetryResponse> Toners,
    DateTime LastUpdate
);
