using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services
{
    public class PrinterService(ApplicationDbContext dbContext)
    {
        public async Task<PrinterResponse> CreatePrinterAsync(CreatePrinterRequest createPrinterRequest)
        {
            bool locationExists = await dbContext.Locations.AnyAsync(l => l.Id == createPrinterRequest.LocationId);
            if (!locationExists) throw new ArgumentException($"Location {createPrinterRequest.LocationId} not found.");
            Printer printer = new(createPrinterRequest.Name, createPrinterRequest.Model, createPrinterRequest.Manufacturer, createPrinterRequest.IpAddress, createPrinterRequest.LocationId);
            dbContext.Printers.Add(printer);
            await dbContext.SaveChangesAsync();
            return new PrinterResponse(printer.Id, printer.Name, printer.Model, printer.Manufacturer, printer.IpAddress, printer.IsActive, printer.LocationId, printer.CreatedAt);
        }
        public async Task<IEnumerable<PrinterResponse>> GetPrintersAsync() => await dbContext.Printers.AsNoTracking().Select(p => new PrinterResponse(p.Id, p.Name, p.Model, p.Manufacturer, p.IpAddress, p.IsActive, p.LocationId, p.CreatedAt)).ToListAsync();
        public async Task<PrinterResponse?> GetPrinterByIdAsync(Guid id) => await dbContext.Printers.AsNoTracking().Where(p => p.Id == id).Select(p => new PrinterResponse(p.Id, p.Name, p.Model, p.Manufacturer, p.IpAddress, p.IsActive, p.LocationId, p.CreatedAt)).FirstOrDefaultAsync();
        public async Task<PrinterResponse?> UpdatePrinterAsync(Guid id, UpdatePrinterRequest updatePrinterRequest)
        {
            Printer? printer = await dbContext.Printers.FindAsync(id);
            if (printer is null) return null;
            if (updatePrinterRequest.Name is not null) printer.UpdateName(updatePrinterRequest.Name);
            if (updatePrinterRequest.Model is not null) printer.UpdateModel(updatePrinterRequest.Model);
            if (updatePrinterRequest.Manufacturer is not null) printer.UpdateManufacturer(updatePrinterRequest.Manufacturer);
            if (updatePrinterRequest.IpAddress is not null) printer.UpdateIpAddress(updatePrinterRequest.IpAddress);
            if (updatePrinterRequest.IsActive.HasValue) printer.SetActiveStatus(updatePrinterRequest.IsActive.Value);
            if (updatePrinterRequest.LocationId.HasValue)
            {
                bool locationExists = await dbContext.Locations.AnyAsync(l => l.Id == updatePrinterRequest.LocationId.Value);
                if (!locationExists) throw new ArgumentException($"Location with ID {updatePrinterRequest.LocationId.Value} does not exist.", nameof(updatePrinterRequest.LocationId));
                printer.UpdateLocationId(updatePrinterRequest.LocationId.Value);
            }
            await dbContext.SaveChangesAsync();
            return new PrinterResponse(printer.Id, printer.Name, printer.Model, printer.Manufacturer, printer.IpAddress, printer.IsActive, printer.LocationId, printer.CreatedAt);
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