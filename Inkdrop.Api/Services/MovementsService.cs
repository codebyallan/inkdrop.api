using Inkdrop.Api.Data;
using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public class MovementsService(ApplicationDbContext context)
{
    public async Task<MovementsResponse> CreateAsync(CreateMovementRequest createMovementRequest)
    {
        if (createMovementRequest.TonerId == Guid.Empty) throw new Exception("TonerId is required");
        Toner toner = await context.Toners.FindAsync(createMovementRequest.TonerId) ?? throw new Exception("Toner not found");
        if (createMovementRequest.Type.Equals("OUT", StringComparison.CurrentCultureIgnoreCase))
        {
            if (createMovementRequest.PrinterId == null || createMovementRequest.PrinterId == Guid.Empty) throw new Exception("PrinterId is required for OUT movements");
            _ = await context.Printers.FindAsync(createMovementRequest.PrinterId) ?? throw new Exception("Printer not found");
            toner.Out(createMovementRequest.Quantity);
        }
        else toner.In(createMovementRequest.Quantity);
        Movements movement = new(createMovementRequest.TonerId, createMovementRequest.PrinterId, createMovementRequest.Quantity, createMovementRequest.Description, createMovementRequest.Type);
        context.Movements.Add(movement);
        await context.SaveChangesAsync();
        return new MovementsResponse(movement.Id, movement.TonerId, movement.PrinterId, movement.Quantity, movement.Description, movement.Type, movement.CreatedAt);
    }
    public async Task<List<MovementsResponse>> GetAllAsync() => await context.Movements.AsNoTracking().Select(m => new MovementsResponse(m.Id, m.TonerId, m.PrinterId, m.Quantity, m.Description, m.Type, m.CreatedAt)).ToListAsync();
    public async Task<MovementsResponse?> GetByIdAsync(Guid id) => await context.Movements.AsNoTracking().Where(m => m.Id == id).Select(m => new MovementsResponse(m.Id, m.TonerId, m.PrinterId, m.Quantity, m.Description, m.Type, m.CreatedAt)).FirstOrDefaultAsync();
}
