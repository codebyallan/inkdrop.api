using System.Collections.Generic;

namespace Inkdrop.Api.DTOs.Responses;

public record PrinterTelemetryResponse(
    int TotalPages,
    int? MonoPages,
    int? ColorPages,
    string Status,
    List<TonerTelemetryResponse> Toners,
    DateTime LastUpdate
);
