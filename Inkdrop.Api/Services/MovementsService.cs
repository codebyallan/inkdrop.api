using Inkdrop.Api.Data;
using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public class MovementsService(ApplicationDbContext context, NotificationContext notificationContext)
{
    public async Task<MovementsResponse?> CreateAsync(CreateMovementRequest request)
    {
        Toner? toner = await context.Toners.FindAsync(request.TonerId);
        if (toner == null) notificationContext.AddNotification("TonerId", "Not found");
        if (request.Type.Equals("OUT", StringComparison.OrdinalIgnoreCase))
        {
            Printer? printer = await context.Printers.FindAsync(request.PrinterId);
            if (printer == null) notificationContext.AddNotification("PrinterId", "Not found");
        }
        if (!notificationContext.IsValid) return null;
        if ("OUT".Equals(request.Type, StringComparison.OrdinalIgnoreCase))
            toner!.Out(request.Quantity);
        else
            toner!.In(request.Quantity);
        if (!toner.IsValid)
        {
            notificationContext.AddNotifications(toner.Notifications);
            return null;
        }
        Movements movement = new(request.TonerId, request.PrinterId, request.Quantity, request.Description, request.Type);
        if (!movement.IsValid)
        {
            notificationContext.AddNotifications(movement.Notifications);
            return null;
        }
        context.Movements.Add(movement);
        await context.SaveChangesAsync();
        return new MovementsResponse(movement.Id, movement.TonerId, movement.PrinterId, movement.Quantity, movement.Description, movement.Type, movement.CreatedAt);
    }
    public async Task<IEnumerable<MovementsResponse>> GetAllAsync() => await context.Movements.AsNoTracking().Select(m => new MovementsResponse(m.Id, m.TonerId, m.PrinterId, m.Quantity, m.Description, m.Type, m.CreatedAt)).ToListAsync();
    public async Task<MovementsResponse?> GetByIdAsync(Guid id) => await context.Movements.AsNoTracking().Where(m => m.Id == id).Select(m => new MovementsResponse(m.Id, m.TonerId, m.PrinterId, m.Quantity, m.Description, m.Type, m.CreatedAt)).FirstOrDefaultAsync();
    public async Task<IEnumerable<MovementsResponse>> GetByPrinterIdAsync(Guid printerId) => await context.Movements.AsNoTracking().Where(m => m.PrinterId == printerId).OrderByDescending(m => m.CreatedAt).Select(m => new MovementsResponse(m.Id, m.TonerId, m.PrinterId, m.Quantity, m.Description, m.Type, m.CreatedAt)).ToListAsync();
    public async Task<IEnumerable<MovementsResponse>> GetByTonerIdAsync(Guid tonerId) => await context.Movements.AsNoTracking().Where(m => m.TonerId == tonerId).OrderByDescending(m => m.CreatedAt).Select(m => new MovementsResponse(m.Id, m.TonerId, m.PrinterId, m.Quantity, m.Description, m.Type, m.CreatedAt)).ToListAsync();

}
