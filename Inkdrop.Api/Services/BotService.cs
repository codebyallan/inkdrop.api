using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Core;
using Inkdrop.Api.Notifications;
using Inkdrop.Api.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public sealed class BotService(ApplicationDbContext dbContext, NotificationContext notificationContext) : IBotService
{
    public async Task<IEnumerable<BotPrinterResponse>> GetMonitoredPrintersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Printers
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new BotPrinterResponse(
                p.Id, 
                p.IpAddress, 
                p.Model, 
                p.Location.Name ?? "Unknown"
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<bool>> SaveTelemetryAsync(TelemetryRequest request, CancellationToken cancellationToken = default)
    {
        var printerExists = await dbContext.Printers.AnyAsync(p => p.Id == request.PrinterId, cancellationToken);
        if (!printerExists)
        {
            notificationContext.AddNotification("PrinterNotFound", "The specified printer was not found in the system.");
            return ServiceResult<bool>.NotFound();
        }

        if (await dbContext.PrinterTelemetries.AnyAsync(t => t.PrinterId == request.PrinterId && t.CollectedAt == request.CollectedAt.ToUniversalTime(), cancellationToken))
        {
            notificationContext.AddNotification("TelemetryDuplicate", "Telemetry data for this printer and timestamp has already been reported.");
            return ServiceResult<bool>.Failure();
        }

        // Monotonicity Check: Pages count must not decrease
        var lastTelemetry = await dbContext.PrinterTelemetries
            .AsNoTracking()
            .Where(t => t.PrinterId == request.PrinterId)
            .OrderByDescending(t => t.CollectedAt)
            .Select(t => new { t.TotalPages, t.MonoPages, t.ColorPages })
            .FirstOrDefaultAsync(cancellationToken);

        if (lastTelemetry != null)
        {
            if (request.TotalPages < lastTelemetry.TotalPages)
            {
                notificationContext.AddNotification("TelemetryPagesRegression", $"Page count regression detected. Current: {request.TotalPages}, Last recorded: {lastTelemetry.TotalPages}.");
                return ServiceResult<bool>.Failure();
            }
            if (request.MonoPages.HasValue && lastTelemetry.MonoPages.HasValue && request.MonoPages < lastTelemetry.MonoPages)
            {
                notificationContext.AddNotification("TelemetryMonoPagesRegression", $"Mono page count regression detected. Current: {request.MonoPages}, Last recorded: {lastTelemetry.MonoPages}.");
                return ServiceResult<bool>.Failure();
            }
            if (request.ColorPages.HasValue && lastTelemetry.ColorPages.HasValue && request.ColorPages < lastTelemetry.ColorPages)
            {
                notificationContext.AddNotification("TelemetryColorPagesRegression", $"Color page count regression detected. Current: {request.ColorPages}, Last recorded: {lastTelemetry.ColorPages}.");
                return ServiceResult<bool>.Failure();
            }
        }

        var telemetry = new PrinterTelemetry(request.PrinterId, request.TotalPages, request.MonoPages, request.ColorPages, request.CollectedAt.ToUniversalTime());
        
        if (!telemetry.IsValid)
        {
            notificationContext.AddNotifications(telemetry);
            return ServiceResult<bool>.Failure();
        }

        foreach (var supplyReq in request.Supplies)
        {
            telemetry.AddSupply(supplyReq.Color, supplyReq.Level);
        }

        if (!telemetry.IsValid)
        {
            notificationContext.AddNotifications(telemetry);
            return ServiceResult<bool>.Failure();
        }

        dbContext.PrinterTelemetries.Add(telemetry);
        
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (!DbExceptionHandler.HandleUniqueConstraintViolation(ex, notificationContext)) throw;
            return ServiceResult<bool>.Failure();
        }

        return ServiceResult<bool>.Success(true);
    }
}
