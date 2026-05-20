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

public sealed class LocationService(ApplicationDbContext dbContext, NotificationContext notificationContext) : ILocationService
{
    public async Task<ServiceResult<LocationResponse>> CreateLocationAsync(CreateLocationRequest createLocationRequest, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Locations.AnyAsync(l => l.Name == createLocationRequest.Name, cancellationToken)) notificationContext.AddNotification("LocationNameAlreadyExists", "A location with the given name already exists.");
        Location? location = new(createLocationRequest.Name, createLocationRequest.Description);
        if (!location.IsValid)
        {
            notificationContext.AddNotifications(location);
            return ServiceResult<LocationResponse>.Failure();
        }
        if (!notificationContext.IsValid) return ServiceResult<LocationResponse>.Failure();
        dbContext.Locations.Add(location);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (!DbExceptionHandler.HandleUniqueConstraintViolation(ex, notificationContext)) throw;
            return ServiceResult<LocationResponse>.Failure();
        }
        return ServiceResult<LocationResponse>.Success(new LocationResponse(location.Id, location.Name, location.Description, location.CreatedAt));
    }

    public async Task<IEnumerable<LocationResponse>> GetAllLocationsAsync(CancellationToken cancellationToken = default) => 
        await dbContext.Locations.AsNoTracking().Select(l => new LocationResponse(l.Id, l.Name, l.Description, l.CreatedAt)).ToListAsync(cancellationToken);

    public async Task<ServiceResult<LocationResponse>> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await dbContext.Locations.AsNoTracking().Where(l => l.Id == id).Select(l => new LocationResponse(l.Id, l.Name, l.Description, l.CreatedAt)).FirstOrDefaultAsync(cancellationToken);
        return location is null ? ServiceResult<LocationResponse>.NotFound() : ServiceResult<LocationResponse>.Success(location);
    }

    public async Task<ServiceResult<LocationResponse>> UpdateLocationAsync(Guid id, UpdateLocationRequest updateLocationRequest, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Locations.AnyAsync(l => l.Name == updateLocationRequest.Name && l.Id != id, cancellationToken)) notificationContext.AddNotification("LocationNameAlreadyExists", "A location with the given name already exists.");
        Location? location = await dbContext.Locations.FindAsync([id], cancellationToken);
        if (location is null) return ServiceResult<LocationResponse>.NotFound();
        location.Update(updateLocationRequest.Name!, updateLocationRequest.Description);
        if (!location.IsValid)
        {
            notificationContext.AddNotifications(location);
            return ServiceResult<LocationResponse>.Failure();
        }
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (DbExceptionHandler.HandleConcurrencyException(ex, notificationContext)) return ServiceResult<LocationResponse>.Failure();
            throw;
        }
        catch (DbUpdateException ex)
        {
            if (!DbExceptionHandler.HandleUniqueConstraintViolation(ex, notificationContext)) throw;
            return ServiceResult<LocationResponse>.Failure();
        }
        return ServiceResult<LocationResponse>.Success(new LocationResponse(location.Id, location.Name, location.Description, location.CreatedAt));
    }

    public async Task<ServiceResult<bool>> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Location? location = await dbContext.Locations.FindAsync([id], cancellationToken);
        if (location is null) return ServiceResult<bool>.NotFound();
        if (await dbContext.Printers.AnyAsync(p => p.LocationId == id, cancellationToken)) notificationContext.AddNotification("LocationHasPrinters", "Cannot delete location because it is associated with existing printers.");
        if (!notificationContext.IsValid) return ServiceResult<bool>.Failure();
        location.MarkAsDeleted();
        await dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<bool>.Success(true);
    }
}
