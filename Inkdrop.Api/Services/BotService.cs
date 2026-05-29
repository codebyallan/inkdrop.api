using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Core;
using Inkdrop.Api.Notifications;
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

        var telemetry = new PrinterTelemetry(request.PrinterId, request.TotalPages, request.CollectedAt);
        
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
        catch (Exception)
        {
            notificationContext.AddNotification("PersistenceError", "An error occurred while saving telemetry data.");
            return ServiceResult<bool>.Failure();
        }

        return ServiceResult<bool>.Success(true);
    }
}
