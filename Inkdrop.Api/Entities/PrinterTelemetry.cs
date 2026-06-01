using Inkdrop.Api.Interfaces;

namespace Inkdrop.Api.Entities;

public sealed class PrinterTelemetry : Base
{
    public Guid PrinterId { get; private set; }
    public int TotalPages { get; private set; }
    public DateTime CollectedAt { get; private set; }
    
    private readonly List<TelemetrySupply> _supplies = new();
    public IReadOnlyCollection<TelemetrySupply> Supplies => _supplies;

    private PrinterTelemetry() { }

    public PrinterTelemetry(Guid printerId, int totalPages, DateTime collectedAt)
    {
        if (printerId == Guid.Empty) AddNotification("TelemetryPrinterInvalid", "Printer ID is required.");
        if (totalPages < 0) AddNotification("TelemetryPagesInvalid", "Total pages cannot be negative.");
        
        // Validate CollectedAt: No MinValue, no dates older than 30 days, no future dates (5 min tolerance)
        if (collectedAt == default || collectedAt == DateTime.MinValue) 
            AddNotification("TelemetryDateInvalid", "Collected date cannot be empty or MinValue.");
        else if (collectedAt < DateTime.UtcNow.AddDays(-30)) 
            AddNotification("TelemetryDateTooOld", "Collected date cannot be older than 30 days.");
        else if (collectedAt > DateTime.UtcNow.AddMinutes(5)) 
            AddNotification("TelemetryDateFuture", "Collected date cannot be in the future.");

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
