using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;

namespace Inkdrop.Api.Interfaces;

public interface IPrinterService
{
    Task<PrinterResponse?> CreatePrinterAsync(CreatePrinterRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<PrinterResponse>> GetAllPrintersAsync(CancellationToken cancellationToken = default);
    Task<PrinterResponse?> GetPrinterByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PrinterResponse?> UpdatePrinterAsync(Guid id, UpdatePrinterRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeletePrinterAsync(Guid id, CancellationToken cancellationToken = default);
}
