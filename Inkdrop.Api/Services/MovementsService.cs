using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Inkdrop.Api.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public sealed class MovementsService(ApplicationDbContext context, NotificationContext notificationContext) : IMovementsService
{
    public async Task<MovementsResponse?> CreateAsync(CreateMovementRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await ExecuteCreateAsync(request, cancellationToken);
            if (result is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }

            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (DbExceptionHandler.HandleConcurrencyException(ex, notificationContext)) return null;
            throw;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<MovementsResponse?> ExecuteCreateAsync(CreateMovementRequest request, CancellationToken cancellationToken)
    {
        Toner? toner = await context.Toners.FindAsync([request.TonerId], cancellationToken);
        if (toner == null) notificationContext.AddNotification("TonerId", "Not found");
        if (request.Type.Equals("OUT", StringComparison.OrdinalIgnoreCase))
        {
            Printer? printer = await context.Printers.FindAsync([request.PrinterId], cancellationToken);
            if (printer == null) notificationContext.AddNotification("PrinterId", "Not found");
        }
        if (!notificationContext.IsValid) return null;
        if ("OUT".Equals(request.Type, StringComparison.OrdinalIgnoreCase))
            toner!.Out(request.Quantity);
        else
            toner!.In(request.Quantity);
        if (!toner.IsValid)
        {
            notificationContext.AddNotifications(toner);
            return null;
        }
        Movements movement = new(request.TonerId, request.PrinterId, request.Quantity, request.Description, request.Type);
        if (!movement.IsValid)
        {
            notificationContext.AddNotifications(movement);
            return null;
        }
        context.Movements.Add(movement);
        await context.SaveChangesAsync(cancellationToken);
        return new MovementsResponse(movement.Id, movement.TonerId, movement.PrinterId, movement.Quantity, movement.Description, movement.Type, movement.CreatedAt);
    }

    public async Task<IEnumerable<MovementsResponse>> GetAllMovementsAsync(CancellationToken cancellationToken = default) => 
        await context.Movements.AsNoTracking().Select(m => new MovementsResponse(m.Id, m.TonerId, m.PrinterId, m.Quantity, m.Description, m.Type, m.CreatedAt)).ToListAsync(cancellationToken);

    public async Task<MovementsResponse?> GetMovementByIdAsync(Guid id, CancellationToken cancellationToken = default) => 
        await context.Movements.AsNoTracking().Where(m => m.Id == id).Select(m => new MovementsResponse(m.Id, m.TonerId, m.PrinterId, m.Quantity, m.Description, m.Type, m.CreatedAt)).FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<MovementsResponse>> GetMovementsByPrinterIdAsync(Guid printerId, CancellationToken cancellationToken = default) => 
        await context.Movements.AsNoTracking().Where(m => m.PrinterId == printerId).OrderByDescending(m => m.CreatedAt).Select(m => new MovementsResponse(m.Id, m.TonerId, m.PrinterId, m.Quantity, m.Description, m.Type, m.CreatedAt)).ToListAsync(cancellationToken);

    public async Task<IEnumerable<MovementsResponse>> GetMovementsByTonerIdAsync(Guid tonerId, CancellationToken cancellationToken = default) => 
        await context.Movements.AsNoTracking().Where(m => m.TonerId == tonerId).OrderByDescending(m => m.CreatedAt).Select(m => new MovementsResponse(m.Id, m.TonerId, m.PrinterId, m.Quantity, m.Description, m.Type, m.CreatedAt)).ToListAsync(cancellationToken);
}
