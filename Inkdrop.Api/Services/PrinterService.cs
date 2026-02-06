using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs;
using Inkdrop.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services
{
    public class PrinterService(ApplicationDbContext dbContext)
    {
        public async Task<Printer> CreatePrinterAsync(CreatePrinterDto createPrinterDto)
        {
            bool locationExists = await dbContext.Locations.AnyAsync(l => l.Id == createPrinterDto.LocationId);
            if (!locationExists) throw new ArgumentException($"Location {createPrinterDto.LocationId} not found.");
            Printer printer = new(createPrinterDto.Name, createPrinterDto.Model, createPrinterDto.Manufacturer, createPrinterDto.IpAddress, createPrinterDto.LocationId);
            dbContext.Printers.Add(printer);
            await dbContext.SaveChangesAsync();
            return printer;
        }
        public async Task<IEnumerable<Printer>> GetPrintersAsync() => await dbContext.Printers.AsNoTracking().ToListAsync();
        public async Task<Printer?> GetPrinterByIdAsync(Guid id) => await dbContext.Printers.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        public async Task<Printer?> UpdatePrinterAsync(Guid id, UpdatePrinterDto updatePrinterDto)
        {
            Printer? printer = await dbContext.Printers.FindAsync(id);
            if (printer is null) return null;
            if (updatePrinterDto.Name is not null) printer.UpdateName(updatePrinterDto.Name);
            if (updatePrinterDto.Model is not null) printer.UpdateModel(updatePrinterDto.Model);
            if (updatePrinterDto.Manufacturer is not null) printer.UpdateManufacturer(updatePrinterDto.Manufacturer);
            if (updatePrinterDto.IpAddress is not null) printer.UpdateIpAddress(updatePrinterDto.IpAddress);
            if (updatePrinterDto.IsActive.HasValue) printer.SetActiveStatus(updatePrinterDto.IsActive.Value);
            if (updatePrinterDto.LocationId.HasValue)
            {
                bool locationExists = await dbContext.Locations.AnyAsync(l => l.Id == updatePrinterDto.LocationId.Value);
                if (!locationExists) throw new ArgumentException($"Location with ID {updatePrinterDto.LocationId.Value} does not exist.", nameof(updatePrinterDto.LocationId));
                printer.UpdateLocationId(updatePrinterDto.LocationId.Value);
            }
            await dbContext.SaveChangesAsync();
            return printer;
        }
        public async Task<bool> DeletePrinterAsync(Guid id)
        {
            Printer? printer = await dbContext.Printers.FindAsync(id);
            if (printer is null) return false;
            printer.MarkAsDeleted();
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}