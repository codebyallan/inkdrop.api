using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs;
using Inkdrop.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public class MovementsService(ApplicationDbContext context)
{
    public async Task<Movements> CreateAsync(CreateMovementDto createMovementDto)
    {
        if (createMovementDto.TonerId == Guid.Empty) throw new Exception("TonerId is required");
        Toner toner = await context.Toners.FindAsync(createMovementDto.TonerId) ?? throw new Exception("Toner not found");
        if (createMovementDto.Type.Equals("OUT", StringComparison.CurrentCultureIgnoreCase))
        {
            if (createMovementDto.PrinterId == null || createMovementDto.PrinterId == Guid.Empty) throw new Exception("PrinterId is required for OUT movements");
            _ = await context.Printers.FindAsync(createMovementDto.PrinterId) ?? throw new Exception("Printer not found");
            toner.Out(createMovementDto.Quantity);
        }
        else toner.In(createMovementDto.Quantity);
        Movements movement = new(createMovementDto.TonerId, createMovementDto.PrinterId, createMovementDto.Quantity, createMovementDto.Description, createMovementDto.Type);
        context.Movements.Add(movement);
        await context.SaveChangesAsync();
        return movement;
    }
    public async Task<List<Movements>> GetAllAsync() => await context.Movements.AsNoTracking().ToListAsync();
    public async Task<Movements?> GetByIdAsync(Guid id) => await context.Movements.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
}
