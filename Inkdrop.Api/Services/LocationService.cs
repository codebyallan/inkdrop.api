using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public class LocationService(ApplicationDbContext dbContext, NotificationContext notificationContext)
{
    public async Task<LocationResponse?> CreateLocationAsync(CreateLocationRequest createLocationRequest)
    {
        Location? location = new(createLocationRequest.Name, createLocationRequest.Description);
        if (!location.IsValid)
        {
            notificationContext.AddNotifications(location);
            return null;
        }
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();
        return new LocationResponse(location.Id, location.Name, location.Description, location.CreatedAt);
    }
    public async Task<IEnumerable<LocationResponse>> GetAllLocationsAsync() => await dbContext.Locations.AsNoTracking().Select(l => new LocationResponse(l.Id, l.Name, l.Description, l.CreatedAt)).ToListAsync();
    public async Task<LocationResponse?> GetLocationByIdAsync(Guid id) => await dbContext.Locations.AsNoTracking().Where(l => l.Id == id).Select(l => new LocationResponse(l.Id, l.Name, l.Description, l.CreatedAt)).FirstOrDefaultAsync();
    public async Task<LocationResponse?> UpdateLocationAsync(Guid id, UpdateLocationRequest updateLocationRequest)
    {
        Location? location = await dbContext.Locations.FindAsync(id);
        if (location is null) return null;
        location.Update(updateLocationRequest.Name!, updateLocationRequest.Description);
        if (!location.IsValid)
        {
            notificationContext.AddNotifications(location);
            return null;
        }
        await dbContext.SaveChangesAsync();
        return new LocationResponse(location.Id, location.Name, location.Description, location.CreatedAt);
    }
    public async Task<bool> DeleteLocationAsync(Guid id)
    {
        Location? location = await dbContext.Locations.FindAsync(id);
        if (location is null) return false;
        if (await dbContext.Printers.AnyAsync(p => p.LocationId == id)) notificationContext.AddNotification("LocationHasPrinters", "Cannot delete location because it is associated with existing printers.");
        if (!notificationContext.IsValid) return false;
        location.MarkAsDeleted();
        await dbContext.SaveChangesAsync();
        return true;
    }

}