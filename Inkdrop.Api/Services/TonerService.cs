using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Inkdrop.Api.Extensions;
using Inkdrop.Api.Core;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public sealed class TonerService(ApplicationDbContext dbContext, NotificationContext notificationContext) : ITonerService
{
    public async Task<ServiceResult<TonerResponse>> CreateTonerAsync(CreateTonerRequest createTonerRequest, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Toners.AnyAsync(t => t.Model == createTonerRequest.Model && t.Manufacturer == createTonerRequest.Manufacturer && t.Color == createTonerRequest.Color, cancellationToken)) notificationContext.AddNotification("TonerAlreadyExists", "A toner with the given model, manufacturer and color already exists.");
        Toner toner = new(createTonerRequest.Model, createTonerRequest.Manufacturer, createTonerRequest.Color);
        if (!toner.IsValid)
        {
            notificationContext.AddNotifications(toner);
            return ServiceResult<TonerResponse>.Failure();
        }
        if (!notificationContext.IsValid) return ServiceResult<TonerResponse>.Failure();
        dbContext.Toners.Add(toner);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (!DbExceptionHandler.HandleUniqueConstraintViolation(ex, notificationContext)) throw;
            return ServiceResult<TonerResponse>.Failure();
        }
        return ServiceResult<TonerResponse>.Success(new TonerResponse(toner.Id, toner.Model, toner.Manufacturer, toner.Color, toner.Quantity, toner.CreatedAt));
    }

    public async Task<IEnumerable<TonerResponse>> GetAllTonersAsync(CancellationToken cancellationToken = default) => 
        await dbContext.Toners.AsNoTracking().Select(t => new TonerResponse(t.Id, t.Model, t.Manufacturer, t.Color, t.Quantity, t.CreatedAt)).ToListAsync(cancellationToken);

    public async Task<ServiceResult<TonerResponse>> GetTonerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var toner = await dbContext.Toners.AsNoTracking().Where(t => t.Id == id).Select(t => new TonerResponse(t.Id, t.Model, t.Manufacturer, t.Color, t.Quantity, t.CreatedAt)).FirstOrDefaultAsync(cancellationToken);
        return toner is null ? ServiceResult<TonerResponse>.NotFound() : ServiceResult<TonerResponse>.Success(toner);
    }

    public async Task<ServiceResult<TonerResponse>> UpdateTonerAsync(Guid id, UpdateTonerRequest updateTonerRequest, CancellationToken cancellationToken = default)
    {
        Toner? toner = await dbContext.Toners.FindAsync([id], cancellationToken);
        if (toner is null) return ServiceResult<TonerResponse>.NotFound();
        string model = updateTonerRequest.Model ?? toner.Model;
        string manufacturer = updateTonerRequest.Manufacturer ?? toner.Manufacturer;
        if (await dbContext.Toners.AnyAsync(t => t.Id != id && t.Model == model && t.Manufacturer == manufacturer && t.Color == toner.Color, cancellationToken)) notificationContext.AddNotification("TonerAlreadyExists", "A toner with the given model, manufacturer and color already exists.");
        if (updateTonerRequest.Model is not null) toner.UpdateModel(updateTonerRequest.Model);
        if (updateTonerRequest.Manufacturer is not null) toner.UpdateManufacturer(updateTonerRequest.Manufacturer);
        if (!toner.IsValid)
        {
            notificationContext.AddNotifications(toner);
        }
        if (!notificationContext.IsValid) return ServiceResult<TonerResponse>.Failure();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (DbExceptionHandler.HandleConcurrencyException(ex, notificationContext)) return ServiceResult<TonerResponse>.Failure();
            throw;
        }
        catch (DbUpdateException ex)
        {
            if (!DbExceptionHandler.HandleUniqueConstraintViolation(ex, notificationContext)) throw;
            return ServiceResult<TonerResponse>.Failure();
        }
        return ServiceResult<TonerResponse>.Success(new TonerResponse(toner.Id, toner.Model, toner.Manufacturer, toner.Color, toner.Quantity, toner.CreatedAt));
    }

    public async Task<ServiceResult<bool>> DeleteTonerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Toner? toner = await dbContext.Toners.FindAsync([id], cancellationToken);
        if (toner is null) return ServiceResult<bool>.NotFound();
        var isMovementAssociated = await dbContext.Movements.AnyAsync(m => m.TonerId == id, cancellationToken);
        if (!isMovementAssociated) toner.MarkAsDeleted();
        else
        {
            notificationContext.AddNotification("TonerAssociatedWithMovements", "Toner is associated with movements and cannot be deleted.");
            return ServiceResult<bool>.Failure();
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<IEnumerable<TonerResponse>> GetLowerTonersAsync(int threshold = 3, CancellationToken cancellationToken = default) => 
        await dbContext.Toners.AsNoTracking().Where(t => t.Quantity <= threshold).Select(t => new TonerResponse(t.Id, t.Model, t.Manufacturer, t.Color, t.Quantity, t.CreatedAt)).ToListAsync(cancellationToken);
}
