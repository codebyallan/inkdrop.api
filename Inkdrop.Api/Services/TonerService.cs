using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs;
using Inkdrop.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services;

public class TonerService(ApplicationDbContext dbContext)
{
    public async Task<Toner> CreateTonerAsync(CreateTonerDto createTonerDto)
    {
        Toner toner = new(createTonerDto.Model, createTonerDto.Manufacturer, createTonerDto.Color);
        dbContext.Toners.Add(toner);
        await dbContext.SaveChangesAsync();
        return toner;
    }
    public async Task<IEnumerable<Toner>> GetAllTonersAsync() => await dbContext.Toners.AsNoTracking().ToListAsync();
    public async Task<Toner?> GetTonerByIdAsync(Guid id) => await dbContext.Toners.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    public async Task<Toner?> UpdateTonerAsync(Guid id, UpdateTonerDto updateTonerDto)
    {
        Toner? toner = await dbContext.Toners.FindAsync(id);
        if (toner is null) return null;
        if (updateTonerDto.Model is not null) toner.UpdateModel(updateTonerDto.Model);
        if (updateTonerDto.Manufacturer is not null) toner.UpdateManufacturer(updateTonerDto.Manufacturer);
        await dbContext.SaveChangesAsync();
        return toner;
    }
    public async Task<bool> DeleteTonerAsync(Guid id)
    {
        Toner? toner = await dbContext.Toners.FindAsync(id);
        if (toner is null) return false;
        toner.MarkAsDeleted();
        await dbContext.SaveChangesAsync();
        return true;
    }
}