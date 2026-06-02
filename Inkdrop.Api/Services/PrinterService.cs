using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Inkdrop.Api.Extensions;
using Inkdrop.Api.Core;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public sealed class PrinterService(ApplicationDbContext dbContext, NotificationContext notificationContext) : IPrinterService
{
    public async Task<ServiceResult<PrinterResponse>> CreatePrinterAsync(CreatePrinterRequest createPrinterRequest, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Printers.AnyAsync(p => p.IpAddress == createPrinterRequest.IpAddress, cancellationToken)) notificationContext.AddNotification("PrinterIpAddressAlreadyExists", "A printer with the given IP address already exists.");
        bool locationExists = await dbContext.Locations.AnyAsync(l => l.Id == createPrinterRequest.LocationId, cancellationToken);
        if (!locationExists) notificationContext.AddNotification("LocationId", "Not found");
        Printer printer = new(createPrinterRequest.Name, createPrinterRequest.Model, createPrinterRequest.Manufacturer, createPrinterRequest.IpAddress, createPrinterRequest.LocationId);
        if (!printer.IsValid)
        {
            notificationContext.AddNotifications(printer);
        }
        if (!notificationContext.IsValid) return ServiceResult<PrinterResponse>.Failure();
        dbContext.Printers.Add(printer);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (!DbExceptionHandler.HandleUniqueConstraintViolation(ex, notificationContext)) throw;
            return ServiceResult<PrinterResponse>.Failure();
        }
        await dbContext.Entry(printer).Reference(p => p.Location).LoadAsync(cancellationToken);
        return ServiceResult<PrinterResponse>.Success(new PrinterResponse(printer.Id, printer.Name, printer.Model, printer.Manufacturer, printer.IpAddress, printer.IsActive, printer.LocationId, printer.Location.Name, printer.CreatedAt));
    }

    public async Task<IEnumerable<PrinterResponse>> GetAllPrintersAsync(CancellationToken cancellationToken = default) 
    {
        var printers = await dbContext.Printers.AsNoTracking()
            .Include(p => p.Location)
            .ToListAsync(cancellationToken);

        var telemetryData = await dbContext.PrinterTelemetries
            .AsNoTracking()
            .Include(t => t.Supplies)
            .GroupBy(t => t.PrinterId)
            .Select(g => g.OrderByDescending(t => t.CollectedAt).FirstOrDefault())
            .ToListAsync(cancellationToken);

        var telemetryMap = telemetryData.ToDictionary(t => t!.PrinterId, t => t!);

        return printers.Select(p => {
            var tel = telemetryMap.GetValueOrDefault(p.Id);
            return new PrinterResponse(
                p.Id, p.Name, p.Model, p.Manufacturer, p.IpAddress, p.IsActive, p.LocationId, p.Location.Name, p.CreatedAt,
                tel == null ? null : new PrinterTelemetryResponse(
                    tel.TotalPages,
                    tel.MonoPages,
                    tel.ColorPages,
                    tel.CollectedAt > DateTime.UtcNow.AddDays(-1) ? "Online" : "Offline",
                    tel.Supplies.Select(s => new TonerTelemetryResponse(s.Color, s.Level)).ToList(),
                    tel.CollectedAt
                )
            );
        });
    }

    public async Task<ServiceResult<PrinterResponse>> GetPrinterByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var printer = await dbContext.Printers.AsNoTracking()
            .Include(p => p.Location)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (printer is null) return ServiceResult<PrinterResponse>.NotFound();

        var tel = await dbContext.PrinterTelemetries
            .AsNoTracking()
            .Include(t => t.Supplies)
            .Where(t => t.PrinterId == id)
            .OrderByDescending(t => t.CollectedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return ServiceResult<PrinterResponse>.Success(new PrinterResponse(
            printer.Id, printer.Name, printer.Model, printer.Manufacturer, printer.IpAddress, printer.IsActive, printer.LocationId, printer.Location.Name, printer.CreatedAt,
            tel == null ? null : new PrinterTelemetryResponse(
                tel.TotalPages,
                tel.MonoPages,
                tel.ColorPages,
                tel.CollectedAt > DateTime.UtcNow.AddDays(-1) ? "Online" : "Offline",
                tel.Supplies.Select(s => new TonerTelemetryResponse(s.Color, s.Level)).ToList(),
                tel.CollectedAt
            )
        ));
    }

    public async Task<ServiceResult<PrinterResponse>> UpdatePrinterAsync(Guid id, UpdatePrinterRequest updatePrinterRequest, CancellationToken cancellationToken = default)
    {
        if (updatePrinterRequest.IpAddress is not null && await dbContext.Printers.AnyAsync(p => p.IpAddress == updatePrinterRequest.IpAddress && p.Id != id, cancellationToken)) notificationContext.AddNotification("PrinterIpAddressAlreadyExists", "A printer with the given IP address already exists.");
        Printer? printer = await dbContext.Printers.FindAsync([id], cancellationToken);
        if (printer is null) return ServiceResult<PrinterResponse>.NotFound();
        if (updatePrinterRequest.Name is not null) printer.UpdateName(updatePrinterRequest.Name);
        if (updatePrinterRequest.Model is not null) printer.UpdateModel(updatePrinterRequest.Model);
        if (updatePrinterRequest.Manufacturer is not null) printer.UpdateManufacturer(updatePrinterRequest.Manufacturer);
        if (updatePrinterRequest.IpAddress is not null) printer.UpdateIpAddress(updatePrinterRequest.IpAddress);
        if (updatePrinterRequest.IsActive.HasValue) printer.SetActiveStatus(updatePrinterRequest.IsActive.Value);
        if (updatePrinterRequest.LocationId.HasValue)
        {
            bool locationExists = await dbContext.Locations.AnyAsync(l => l.Id == updatePrinterRequest.LocationId.Value, cancellationToken);
            if (!locationExists) notificationContext.AddNotification("LocationId", "Not found");
            else printer.UpdateLocationId(updatePrinterRequest.LocationId.Value);
        }
        if (!printer.IsValid)
        {
            notificationContext.AddNotifications(printer);
        }
        if (!notificationContext.IsValid) return ServiceResult<PrinterResponse>.Failure();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (DbExceptionHandler.HandleConcurrencyException(ex, notificationContext)) return ServiceResult<PrinterResponse>.Failure();
            throw;
        }
        catch (DbUpdateException ex)
        {
            if (!DbExceptionHandler.HandleUniqueConstraintViolation(ex, notificationContext)) throw;
            return ServiceResult<PrinterResponse>.Failure();
        }
        await dbContext.Entry(printer).Reference(p => p.Location).LoadAsync(cancellationToken);
        return ServiceResult<PrinterResponse>.Success(new PrinterResponse(printer.Id, printer.Name, printer.Model, printer.Manufacturer, printer.IpAddress, printer.IsActive, printer.LocationId, printer.Location.Name, printer.CreatedAt));
    }

    public async Task<ServiceResult<bool>> DeletePrinterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Printer? printer = await dbContext.Printers.FindAsync([id], cancellationToken);
        if (printer is null) return ServiceResult<bool>.NotFound();
        var isMovementAssociated = await dbContext.Movements.AnyAsync(m => m.PrinterId == id, cancellationToken);
        if (!isMovementAssociated) printer.MarkAsDeleted();
        else
        {
            notificationContext.AddNotification("PrinterIsInUse", "Printer is associated with movements and cannot be deleted.");
            return ServiceResult<bool>.Failure();
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<bool>.Success(true);
    }
}
