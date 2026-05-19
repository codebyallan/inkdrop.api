using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public sealed class LocationService(ApplicationDbContext dbContext, NotificationContext notificationContext) : ILocationService
{
    public async Task<LocationResponse?> CreateLocationAsync(CreateLocationRequest createLocationRequest, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Locations.AnyAsync(l => l.Name == createLocationRequest.Name, cancellationToken)) notificationContext.AddNotification("LocationNameAlreadyExists", "A location with the given name already exists.");
        Location? location = new(createLocationRequest.Name, createLocationRequest.Description);
        if (!location.IsValid)
        {
            notificationContext.AddNotifications(location);
            return null;
        }
        if (!notificationContext.IsValid) return null;
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new LocationResponse(location.Id, location.Name, location.Description, location.CreatedAt);
    }

    public async Task<IEnumerable<LocationResponse>> GetAllLocationsAsync(CancellationToken cancellationToken = default) => 
        await dbContext.Locations.AsNoTracking().Select(l => new LocationResponse(l.Id, l.Name, l.Description, l.CreatedAt)).ToListAsync(cancellationToken);

    public async Task<LocationResponse?> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default) => 
        await dbContext.Locations.AsNoTracking().Where(l => l.Id == id).Select(l => new LocationResponse(l.Id, l.Name, l.Description, l.CreatedAt)).FirstOrDefaultAsync(cancellationToken);

    public async Task<LocationResponse?> UpdateLocationAsync(Guid id, UpdateLocationRequest updateLocationRequest, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Locations.AnyAsync(l => l.Name == updateLocationRequest.Name && l.Id != id, cancellationToken)) notificationContext.AddNotification("LocationNameAlreadyExists", "A location with the given name already exists.");
        Location? location = await dbContext.Locations.FindAsync([id], cancellationToken);
        if (location is null) return null;
        location.Update(updateLocationRequest.Name!, updateLocationRequest.Description);
        if (!location.IsValid)
        {
            notificationContext.AddNotifications(location);
            return null;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return new LocationResponse(location.Id, location.Name, location.Description, location.CreatedAt);
    }

    public async Task<bool> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Location? location = await dbContext.Locations.FindAsync([id], cancellationToken);
        if (location is null) return false;
        if (await dbContext.Printers.AnyAsync(p => p.LocationId == id, cancellationToken)) notificationContext.AddNotification("LocationHasPrinters", "Cannot delete location because it is associated with existing printers.");
        if (!notificationContext.IsValid) return false;
        location.MarkAsDeleted();
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
