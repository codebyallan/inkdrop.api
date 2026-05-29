using Inkdrop.Api.Interfaces;

namespace Inkdrop.Api.Entities;

public sealed class PrinterTelemetry : Base
{
    public Guid PrinterId { get; private set; }
    public int TotalPages { get; private set; }
    public DateTime CollectedAt { get; private set; }
    
    private readonly List<TelemetrySupply> _supplies = new();
    public virtual IReadOnlyCollection<TelemetrySupply> Supplies => _supplies;

    private PrinterTelemetry() { }

    public PrinterTelemetry(Guid printerId, int totalPages, DateTime collectedAt)
    {
        if (printerId == Guid.Empty) AddNotification("TelemetryPrinterInvalid", "Printer ID is required.");
        if (totalPages < 0) AddNotification("TelemetryPagesInvalid", "Total pages cannot be negative.");

        if (!IsValid) return;

        PrinterId = printerId;
        TotalPages = totalPages;
        CollectedAt = collectedAt;
    }

    public void AddSupply(string color, int level)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            AddNotification("SupplyColorInvalid", "Supply color cannot be empty.");
            return;
        }
        if (level < 0 || level > 100)
        {
            AddNotification("SupplyLevelInvalid", "Supply level must be between 0 and 100.");
            return;
        }

        _supplies.Add(new TelemetrySupply(this, color, level));
    }
}
