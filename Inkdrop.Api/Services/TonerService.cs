using Inkdrop.Api.Data;
using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public class TonerService(ApplicationDbContext dbContext, NotificationContext notificationContext)
{
    public async Task<TonerResponse?> CreateTonerAsync(CreateTonerRequest createTonerRequest)
    {
        if (await dbContext.Toners.AnyAsync(t => t.Model == createTonerRequest.Model && t.Manufacturer == createTonerRequest.Manufacturer && t.Color == createTonerRequest.Color)) notificationContext.AddNotification("TonerAlreadyExists", "A toner with the given model, manufacturer and color already exists.");
        Toner toner = new(createTonerRequest.Model, createTonerRequest.Manufacturer, createTonerRequest.Color);
        if (!toner.IsValid)
        {
            notificationContext.AddNotifications(toner);
            return null;
        }
        dbContext.Toners.Add(toner);
        await dbContext.SaveChangesAsync();
        return new TonerResponse(toner.Id, toner.Model, toner.Manufacturer, toner.Color, toner.Quantity, toner.CreatedAt);
    }
    public async Task<IEnumerable<TonerResponse>> GetAllTonersAsync() => await dbContext.Toners.AsNoTracking().Select(t => new TonerResponse(t.Id, t.Model, t.Manufacturer, t.Color, t.Quantity, t.CreatedAt)).ToListAsync();
    public async Task<TonerResponse?> GetTonerByIdAsync(Guid id) => await dbContext.Toners.AsNoTracking().Where(t => t.Id == id).Select(t => new TonerResponse(t.Id, t.Model, t.Manufacturer, t.Color, t.Quantity, t.CreatedAt)).FirstOrDefaultAsync();
    public async Task<TonerResponse?> UpdateTonerAsync(Guid id, UpdateTonerRequest updateTonerRequest)
    {
        Toner? toner = await dbContext.Toners.FindAsync(id);
        if (toner is null) return null;
        string model = updateTonerRequest.Model ?? toner.Model;
        string manufacturer = updateTonerRequest.Manufacturer ?? toner.Manufacturer;
        if (await dbContext.Toners.AnyAsync(t => t.Id != id && t.Model == model && t.Manufacturer == manufacturer && t.Color == toner.Color)) notificationContext.AddNotification("TonerAlreadyExists", "A toner with the given model, manufacturer and color already exists.");
        if (updateTonerRequest.Model is not null) toner.UpdateModel(updateTonerRequest.Model);
        if (updateTonerRequest.Manufacturer is not null) toner.UpdateManufacturer(updateTonerRequest.Manufacturer);
        notificationContext.AddNotifications(toner);
        if (!notificationContext.IsValid) return null;
        await dbContext.SaveChangesAsync();
        return new TonerResponse(toner.Id, toner.Model, toner.Manufacturer, toner.Color, toner.Quantity, toner.CreatedAt);
    }
    public async Task<bool> DeleteTonerAsync(Guid id)
    {
        Toner? toner = await dbContext.Toners.FindAsync(id);
        if (toner is null) return false;
        var isMovementAssociated = await dbContext.Movements.AnyAsync(m => m.TonerId == id);
        if (!isMovementAssociated) toner.MarkAsDeleted();
        else
        {
            notificationContext.AddNotification("TonerAssociatedWithMovements", "Toner is associated with movements and cannot be deleted.");
            return false;
        }
        await dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<IEnumerable<TonerResponse>> GetLowerToners(int threshold = 3) => await dbContext.Toners.AsNoTracking().Where(t => t.Quantity <= threshold).Select(t => new TonerResponse(t.Id, t.Model, t.Manufacturer, t.Color, t.Quantity, t.CreatedAt)).ToListAsync();
}