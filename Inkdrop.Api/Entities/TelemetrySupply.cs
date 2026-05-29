using Inkdrop.Api.Interfaces;

namespace Inkdrop.Api.Entities;

public sealed class TelemetrySupply : Base
{
    public Guid TelemetryId { get; private set; }
    public string Color { get; private set; } = string.Empty;
    public int Level { get; private set; }

    private TelemetrySupply() { }

    public TelemetrySupply(PrinterTelemetry telemetry, string color, int level)
    {
        TelemetryId = telemetry.Id;
        Color = color;
        Level = level;
    }
}
