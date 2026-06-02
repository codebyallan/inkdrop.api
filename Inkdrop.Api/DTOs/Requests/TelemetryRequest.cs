using Inkdrop.Api.DTOs.Requests;

namespace Inkdrop.Api.DTOs.Requests;

public record TelemetryRequest(
    Guid PrinterId,
    int TotalPages,
    DateTime CollectedAt,
    List<TelemetrySupplyRequest> Supplies,
    int? ColorPages,
    int? MonoPages
);

public record TelemetrySupplyRequest(string Color, int Level);
