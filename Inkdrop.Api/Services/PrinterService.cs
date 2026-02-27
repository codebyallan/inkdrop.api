using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Services
{
    public class PrinterService(ApplicationDbContext dbContext, NotificationContext notificationContext)
    {
        public async Task<PrinterResponse?> CreatePrinterAsync(CreatePrinterRequest createPrinterRequest)
        {
            if (await dbContext.Printers.AnyAsync(p => p.IpAddress == createPrinterRequest.IpAddress)) notificationContext.AddNotification("PrinterIpAddressAlreadyExists", "A printer with the given IP address already exists.");
            bool locationExists = await dbContext.Locations.AnyAsync(l => l.Id == createPrinterRequest.LocationId);
            if (!locationExists) notificationContext.AddNotification("LocationId", "Not found");
            Printer printer = new(createPrinterRequest.Name, createPrinterRequest.Model, createPrinterRequest.Manufacturer, createPrinterRequest.IpAddress, createPrinterRequest.LocationId);
            if (!printer.IsValid)
            {
                notificationContext.AddNotifications(printer);
            }
            if (!notificationContext.IsValid) return null;
            dbContext.Printers.Add(printer);
            await dbContext.SaveChangesAsync();
            await dbContext.Entry(printer).Reference(p => p.Location).LoadAsync();
            return new PrinterResponse(printer.Id, printer.Name, printer.Model, printer.Manufacturer, printer.IpAddress, printer.IsActive, printer.LocationId, printer.Location.Name, printer.CreatedAt);
        }
        public async Task<IEnumerable<PrinterResponse>> GetPrintersAsync() => await dbContext.Printers.AsNoTracking().Include(p => p.Location).Select(p => new PrinterResponse(p.Id, p.Name, p.Model, p.Manufacturer, p.IpAddress, p.IsActive, p.LocationId, p.Location.Name, p.CreatedAt)).ToListAsync();
        public async Task<PrinterResponse?> GetPrinterByIdAsync(Guid id) => await dbContext.Printers.AsNoTracking().Include(p => p.Location).Where(p => p.Id == id).Select(p => new PrinterResponse(p.Id, p.Name, p.Model, p.Manufacturer, p.IpAddress, p.IsActive, p.LocationId, p.Location.Name, p.CreatedAt)).FirstOrDefaultAsync();
        public async Task<PrinterResponse?> UpdatePrinterAsync(Guid id, UpdatePrinterRequest updatePrinterRequest)
        {
            if (updatePrinterRequest.IpAddress is not null && await dbContext.Printers.AnyAsync(p => p.IpAddress == updatePrinterRequest.IpAddress && p.Id != id)) notificationContext.AddNotification("PrinterIpAddressAlreadyExists", "A printer with the given IP address already exists.");
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
                if (!locationExists) notificationContext.AddNotification("LocationId", "Not found");
                else printer.UpdateLocationId(updatePrinterRequest.LocationId.Value);
            }
            if (!printer.IsValid)
            {
                notificationContext.AddNotifications(printer.Notifications);
            }
            if (!notificationContext.IsValid) return null;
            await dbContext.SaveChangesAsync();
            await dbContext.Entry(printer).Reference(p => p.Location).LoadAsync();
            return new PrinterResponse(printer.Id, printer.Name, printer.Model, printer.Manufacturer, printer.IpAddress, printer.IsActive, printer.LocationId, printer.Location.Name, printer.CreatedAt);
        }
        public async Task<bool> DeletePrinterAsync(Guid id)
        {
            Printer? printer = await dbContext.Printers.FindAsync(id);
            if (printer is null) return false;
            var isMovementAssociated = await dbContext.Movements.AnyAsync(m => m.PrinterId == id);
            if (!isMovementAssociated) printer.MarkAsDeleted();
            else
            {
                notificationContext.AddNotification("PrinterIsInUse", "Printer is associated with movements and cannot be deleted.");
                return false;
            }
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}