using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs;
using Inkdrop.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public class LocationService(ApplicationDbContext dbContext)
{
    public async Task<Location> CreateLocationAsync(CreateLocationDto createLocationDto)
    {
        Location? location = new(createLocationDto.Name, createLocationDto.Description);
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();
        return location;
    }
    public async Task<IEnumerable<Location>> GetAllLocationsAsync() => await dbContext.Locations.AsNoTracking().ToListAsync();
    public async Task<Location?> GetLocationByIdAsync(Guid id) => await dbContext.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
    public async Task<Location?> UpdateLocationAsync(Guid id, UpdateLocationDto updateLocationDto)
    {
        Location? location = await dbContext.Locations.FindAsync(id);
        if (location is null) return null;
        location.Update(updateLocationDto.Name!, updateLocationDto.Description);
        await dbContext.SaveChangesAsync();
        return location;
    }
    public async Task<bool> DeleteLocationAsync(Guid id)
    {
        Location? location = await dbContext.Locations.FindAsync(id);
        if (location is null) return false;
        location.MarkAsDeleted();
        await dbContext.SaveChangesAsync();
        return true;
    }

}