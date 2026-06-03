using Microsoft.EntityFrameworkCore;
using Inkdrop.Api.Data;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Core;
using Inkdrop.Api.Entities;
using Inkdrop.Api.DTOs.Responses;

namespace Inkdrop.Api.Services;

public sealed class ReportsService : IReportsService
{
    private readonly ApplicationDbContext _context;

    public ReportsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<ReportResponse>> GetPageVolumeAsync(DateTime? startDate, DateTime? endDate, Guid? printerId = null, CancellationToken cancellationToken = default)
    {
        var start = (startDate?.ToUniversalTime()) ?? DateTime.MinValue.ToUniversalTime();
        var end = (endDate?.ToUniversalTime()) ?? DateTime.UtcNow;

        var query = _context.PrinterTelemetries
            .AsNoTracking()
            .Where(t => t.CollectedAt >= start && t.CollectedAt <= end);

        if (printerId.HasValue)
        {
            query = query.Where(t => t.PrinterId == printerId.Value);
        }

        var data = await query
            .OrderBy(t => t.CollectedAt)
            .Select(t => new { t.CollectedAt, t.TotalPages, t.PrinterId })
            .ToListAsync(cancellationToken);

        if (!data.Any()) return ServiceResult<ReportResponse>.Success(new ReportResponse(new(), new(), new(0, 0, "stable", "pages")));

        var labels = data.Select(d => d.CollectedAt.ToString("yyyy-MM-dd")).Distinct().ToList();
        
        // Group by printer to create datasets and calculate real volume
        var printerGroups = data.GroupBy(d => d.PrinterId).ToList();
        var datasets = new List<ReportDataset>();
        double totalFleetVolume = 0;

        foreach (var group in printerGroups)
        {
            var pid = group.Key;
            var printer = await _context.Printers
                .AsNoTracking()
                .Include(p => p.Location)
                .FirstOrDefaultAsync(p => p.Id == pid, cancellationToken);
            var sortedGroup = group.OrderBy(d => d.CollectedAt).ToList();
            
            // Printer volume: Last - First
            totalFleetVolume += (sortedGroup.Last().TotalPages - sortedGroup.First().TotalPages);

            var displayName = printer != null 
                ? (printer.Location != null ? $"{printer.Name} - {printer.Location.Name}" : printer.Name) 
                : "Unknown";

            datasets.Add(new ReportDataset(
                displayName,
                labels.Select(l => (double)(sortedGroup.FirstOrDefault(d => d.CollectedAt.ToString("yyyy-MM-dd") == l)?.TotalPages ?? 0)).ToList()
            ));
        }

        // Calculate average pages per day
        var daysDiff = (end - start).TotalDays;
        double avgPerDay = daysDiff > 0 ? totalFleetVolume / daysDiff : totalFleetVolume;

        return ServiceResult<ReportResponse>.Success(new ReportResponse(
            labels,
            datasets,
            new ReportSummary(totalFleetVolume, avgPerDay, totalFleetVolume > 0 ? "up" : "stable", "pages")
        ));
    }

    public async Task<ServiceResult<ReportResponse>> GetTonerConsumptionAsync(DateTime? startDate, DateTime? endDate, Guid? printerId = null, CancellationToken cancellationToken = default)
    {
        var start = (startDate?.ToUniversalTime()) ?? DateTime.MinValue.ToUniversalTime();
        var end = (endDate?.ToUniversalTime()) ?? DateTime.UtcNow;

        var query = _context.PrinterTelemetries
            .AsNoTracking()
            .Include(t => t.Supplies)
            .Where(t => t.CollectedAt >= start && t.CollectedAt <= end);

        if (printerId.HasValue)
        {
            query = query.Where(t => t.PrinterId == printerId.Value);
        }

        var data = await query.OrderBy(t => t.CollectedAt).ToListAsync(cancellationToken);
        if (!data.Any()) return ServiceResult<ReportResponse>.Success(new ReportResponse(new(), new(), new(0, 0, "stable", "%")));

        var labels = data.Select(d => d.CollectedAt.ToString("yyyy-MM-dd")).Distinct().ToList();
        var colors = data.SelectMany(d => d.Supplies)
            .Where(s => IsToner(s.Color))
            .Select(s => s.Color).Distinct().ToList();
        
        var datasets = new List<ReportDataset>();
        double totalDrop = 0;

        foreach (var color in colors)
        {
            var dailyAverages = labels.Select(l => {
                var dayTelemetries = data.Where(d => d.CollectedAt.ToString("yyyy-MM-dd") == l).ToList();
                var levels = dayTelemetries
                    .Select(t => t.Supplies.FirstOrDefault(s => s.Color == color)?.Level)
                    .Where(lvl => lvl.HasValue)
                    .Select(lvl => (double)lvl!.Value)
                    .ToList();
                
                return levels.Any() ? levels.Average() : (double?)null;
            }).ToList();

            var consumptionData = new List<double>();
            double cumulativeConsumption = 0;
            double? previousAverage = null;

            foreach (var currentAverage in dailyAverages)
            {
                if (currentAverage.HasValue)
                {
                    if (previousAverage.HasValue)
                    {
                        var diff = previousAverage.Value - currentAverage.Value;
                        if (diff > 0) cumulativeConsumption += diff;
                    }
                    previousAverage = currentAverage;
                }
                consumptionData.Add(cumulativeConsumption);
            }

            datasets.Add(new ReportDataset(color, consumptionData));
            totalDrop += cumulativeConsumption;
        }

        // --- BI Trend Analysis ---
        var duration = end - start;
        string trend = "stable";

        // Prevent ArgumentOutOfRangeException if start is DateTime.MinValue
        if (start > DateTime.MinValue)
        {
            var prevEnd = start;
            var prevStart = start.Subtract(duration);
            
            double previousDrop = await CalculateTotalConsumptionAsync(prevStart, prevEnd, printerId, cancellationToken);
            
            if (previousDrop > 0)
            {
                if (totalDrop > previousDrop * 1.05) trend = "up";
                else if (totalDrop < previousDrop * 0.95) trend = "down";
            }
            else if (totalDrop > 0)
            {
                trend = "up"; // Was 0, now consuming
            }
        }
        else if (totalDrop > 0)
        {
            trend = "up"; // No previous period to compare, but current consumption exists
        }

        // Calculate average pages per day based on ACTUAL data span, not filter range
        var firstCollected = data.Min(d => d.CollectedAt);
        var lastCollected = data.Max(d => d.CollectedAt);
        var actualDaysDiff = (lastCollected - firstCollected).TotalDays;
        
        // Ensure a minimum of 1 day to avoid division by zero and accurately reflect daily rate
        double effectiveDays = Math.Max(1.0, actualDaysDiff);
        double avgDailyDrop = (colors.Any()) 
            ? totalDrop / (effectiveDays * colors.Count) 
            : 0;

        return ServiceResult<ReportResponse>.Success(new ReportResponse(
            labels,
            datasets,
            new ReportSummary(totalDrop, avgDailyDrop, trend, "%")
        ));
    }

    private async Task<double> CalculateTotalConsumptionAsync(DateTime start, DateTime end, Guid? printerId, CancellationToken cancellationToken)
    {
        var query = _context.PrinterTelemetries
            .AsNoTracking()
            .Include(t => t.Supplies)
            .Where(t => t.CollectedAt >= start && t.CollectedAt <= end);

        if (printerId.HasValue)
            query = query.Where(t => t.PrinterId == printerId.Value);

        var data = await query.OrderBy(t => t.CollectedAt).ToListAsync(cancellationToken);
        if (!data.Any()) return 0;

        double totalConsumption = 0;
        var printerGroups = data.GroupBy(t => t.PrinterId);

        foreach (var group in printerGroups)
        {
            var sorted = group.OrderBy(t => t.CollectedAt).ToList();
            var first = sorted.First();
            var last = sorted.Last();

            foreach (var firstSupply in first.Supplies)
            {
                if (IsToner(firstSupply.Color))
                {
                    var lastSupply = last.Supplies.FirstOrDefault(s => s.Color == firstSupply.Color);
                    if (lastSupply != null)
                    {
                        var diff = firstSupply.Level - lastSupply.Level;
                        if (diff > 0) totalConsumption += diff;
                    }
                }
            }
        }

        return totalConsumption;
    }

    public async Task<ServiceResult<List<PredictiveMetricResponse>>> GetPredictiveAnalysisAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? printerId = null,
        CancellationToken cancellationToken = default)
    {
        var start = (startDate?.ToUniversalTime()) ?? DateTime.UtcNow.AddDays(-14);
        var end = (endDate?.ToUniversalTime()) ?? DateTime.UtcNow;
        var windowDays = (end - start).TotalDays;

        IQueryable<Printer> printersQuery = _context.Printers
            .AsNoTracking()
            .Include(p => p.Location);
        if (printerId.HasValue)
            printersQuery = printersQuery.Where(p => p.Id == printerId.Value);
        
        var printers = await printersQuery.ToListAsync(cancellationToken);
        if (!printers.Any()) return ServiceResult<List<PredictiveMetricResponse>>.Success(new());

        // Projection: Only load required fields to optimize memory and performance
        var telemetriesQuery = _context.PrinterTelemetries
            .AsNoTracking()
            .Where(t => t.CollectedAt >= start && t.CollectedAt <= end);

        if (printerId.HasValue)
            telemetriesQuery = telemetriesQuery.Where(t => t.PrinterId == printerId.Value);

        var allRecentTelemetries = await telemetriesQuery
            .Select(t => new
            {
                t.PrinterId,
                t.CollectedAt,
                Supplies = t.Supplies.Select(s => new { s.Color, s.Level })
            })
            .OrderByDescending(t => t.CollectedAt)
            .ToListAsync(cancellationToken);

        var predictions = new List<PredictiveMetricResponse>();
        var telemetriesByPrinter = allRecentTelemetries.GroupBy(t => t.PrinterId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var printer in printers)
        {
            if (!telemetriesByPrinter.TryGetValue(printer.Id, out var printerTelemetries) || printerTelemetries.Count < 2) 
                continue;

            // Check for minimum data span (at least 24 hours) to avoid volatile predictions
            var latest = printerTelemetries.First().CollectedAt;
            var earliest = printerTelemetries.Last().CollectedAt;
            var daysDiff = (latest - earliest).TotalDays;

            if (daysDiff < 1.0) continue; // Insufficient time span for daily prediction

            // Identify monitored toners
            var colors = printerTelemetries.First().Supplies
                .Where(s => IsToner(s.Color))
                .Select(s => s.Color).ToList();

            foreach (var color in colors)
            {
                // Get first and last levels in the window
                var firstLevel = printerTelemetries.Last().Supplies.FirstOrDefault(s => s.Color == color)?.Level;
                var lastLevel = printerTelemetries.First().Supplies.FirstOrDefault(s => s.Color == color)?.Level;

                if (firstLevel == null || lastLevel == null) continue;

                // Calculate Burn Rate (consumption per day)
                var totalDrop = firstLevel.Value - lastLevel.Value;
                var dailyDrop = totalDrop / daysDiff;

                if (dailyDrop <= 0) continue; // Toner is not being consumed or was replaced

                var daysRemaining = (int)(lastLevel.Value / dailyDrop);
                
                // Dynamic Confidence Score:
                // - Higher if we have more days of data (up to windowDays)
                // - Higher if we have more samples (density)
                double timeFactor = Math.Min(daysDiff / windowDays, 1.0) * 0.6;
                double densityFactor = Math.Min(printerTelemetries.Count / (windowDays * 2.0), 1.0) * 0.4;
                double confidence = Math.Clamp(timeFactor + densityFactor, 0.1, 0.95);

                var displayName = printer.Location != null 
                    ? $"{printer.Name} - {printer.Location.Name}" 
                    : printer.Name;

                predictions.Add(new PredictiveMetricResponse(
                    printer.Id,
                    displayName,
                    color,
                    daysRemaining,
                    DateTime.UtcNow.AddDays(daysRemaining),
                    confidence
                ));
            }
        }

        return ServiceResult<List<PredictiveMetricResponse>>.Success(predictions);
    }

    public async Task<ServiceResult<ExecutiveSummaryResponse>> GetExecutiveSummaryAsync(DateTime? startDate, DateTime? endDate, Guid? printerId = null, CancellationToken cancellationToken = default)
    {
        var start = (startDate?.ToUniversalTime()) ?? DateTime.MinValue.ToUniversalTime();
        var end = (endDate?.ToUniversalTime()) ?? DateTime.UtcNow;

        var query = _context.PrinterTelemetries
            .AsNoTracking()
            .Include(t => t.Supplies)
            .Where(t => t.CollectedAt >= start && t.CollectedAt <= end);

        if (printerId.HasValue)
            query = query.Where(t => t.PrinterId == printerId.Value);

        var telemetries = await query.ToListAsync(cancellationToken);

        if (!telemetries.Any()) 
            return ServiceResult<ExecutiveSummaryResponse>.Success(new ExecutiveSummaryResponse(0, 0, "N/A", 0));

        // 1. Total Pages
        var printerGroups = telemetries.GroupBy(t => t.PrinterId).ToList();
        double totalPages = 0;
        var consumptionMap = new Dictionary<Guid, int>();

        foreach (var group in printerGroups)
        {
            var sorted = group.OrderBy(t => t.CollectedAt).ToList();
            var diff = sorted.Last().TotalPages - sorted.First().TotalPages;
            totalPages += diff;
            consumptionMap[group.Key] = diff;
        }

        // 2. Top Consumer - Only if there was actual consumption in the period
        string topPrinterName = "N/A";
        if (totalPages > 0)
        {
            var topPrinterId = consumptionMap.OrderByDescending(x => x.Value).FirstOrDefault().Key;
            var topPrinter = await _context.Printers
                .AsNoTracking()
                .Include(p => p.Location)
                .FirstOrDefaultAsync(p => p.Id == topPrinterId, cancellationToken);
            
            topPrinterName = topPrinter != null 
                ? (topPrinter.Location != null ? $"{topPrinter.Name} - {topPrinter.Location.Name}" : topPrinter.Name) 
                : "N/A";
        }

        // 3. Fleet Health (Avg level of latest supplies within the period)
        var latestTelemetries = telemetries
            .GroupBy(t => t.PrinterId)
            .Select(g => g.OrderByDescending(t => t.CollectedAt).First())
            .ToList();

        var allLatestToners = latestTelemetries
            .SelectMany(t => t.Supplies)
            .Where(s => IsToner(s.Color))
            .ToList();
        double avgHealth = allLatestToners.Any() ? allLatestToners.Average(s => s.Level) : 0;

        // 4. Critical Toners (< 5%) based on the latest telemetry of the period
        int criticalCount = allLatestToners.Count(s => s.Level < 5);

        return ServiceResult<ExecutiveSummaryResponse>.Success(new ExecutiveSummaryResponse(
            totalPages,
            avgHealth,
            topPrinterName,
            criticalCount
        ));
    }

    public async Task<ServiceResult<PrintedPagesResponse>> GetPrintedPagesAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? printerId,
        CancellationToken cancellationToken = default)
    {
        var start = (startDate?.ToUniversalTime()) ?? DateTime.MinValue.ToUniversalTime();
        var end   = (endDate?.ToUniversalTime())   ?? DateTime.UtcNow;

        var query = _context.PrinterTelemetries
            .AsNoTracking()
            .Where(t => t.CollectedAt >= start && t.CollectedAt <= end);

        if (printerId.HasValue)
            query = query.Where(t => t.PrinterId == printerId.Value);

        var data = await query
            .OrderBy(t => t.CollectedAt)
            .Select(t => new
            {
                t.PrinterId,
                t.CollectedAt,
                t.TotalPages,
                t.MonoPages,
                t.ColorPages
            })
            .ToListAsync(cancellationToken);

        if (!data.Any())
            return ServiceResult<PrintedPagesResponse>.Success(
                new PrintedPagesResponse(0, null, null, start, end, []));

        var printerIds = data.Select(d => d.PrinterId).Distinct().ToList();
        var printers = await _context.Printers
            .AsNoTracking()
            .Where(p => printerIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, LocationName = p.Location.Name })
            .ToListAsync(cancellationToken);

        var printerMap = printers.ToDictionary(
            p => p.Id, 
            p => p.LocationName != null ? $"{p.Name} - {p.LocationName}" : p.Name
        );
        var breakdowns = new List<PrinterPageBreakdown>();

        double fleetTotal = 0;
        double? fleetMono  = null;
        double? fleetColor = null;

        foreach (var group in data.GroupBy(d => d.PrinterId))
        {
            var sorted = group.OrderBy(d => d.CollectedAt).ToList();
            var first  = sorted.First();
            var last   = sorted.Last();

            double printerTotal = last.TotalPages - first.TotalPages;

            double? printerMono = (last.MonoPages.HasValue && first.MonoPages.HasValue)
                ? (double?)(last.MonoPages.Value - first.MonoPages.Value)
                : null;

            double? printerColor = (last.ColorPages.HasValue && first.ColorPages.HasValue)
                ? (double?)(last.ColorPages.Value - first.ColorPages.Value)
                : null;

            fleetTotal += printerTotal;

            if (printerMono.HasValue)
                fleetMono = (fleetMono ?? 0) + printerMono.Value;

            if (printerColor.HasValue)
                fleetColor = (fleetColor ?? 0) + printerColor.Value;

            breakdowns.Add(new PrinterPageBreakdown(
                group.Key,
                printerMap.GetValueOrDefault(group.Key, "Unknown"),
                printerTotal,
                printerMono,
                printerColor
            ));
        }

        return ServiceResult<PrintedPagesResponse>.Success(
            new PrintedPagesResponse(fleetTotal, fleetMono, fleetColor, start, end, breakdowns));
    }

    private bool IsToner(string supplyName)
    {
        if (string.IsNullOrWhiteSpace(supplyName)) return false;
        
        var excludedKeywords = new[] { "drum", "tambor", "developer", "revelador", "fuser", "fusor", "feeder", "alimentador", "waste", "recolha", "cylinder", "cilindro" };
        
        return !excludedKeywords.Any(keyword => supplyName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
