using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Core;

namespace Inkdrop.Api.Interfaces;

public interface IPrinterService
{
    Task<ServiceResult<PrinterResponse>> CreatePrinterAsync(CreatePrinterRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<PrinterResponse>> GetAllPrintersAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<PrinterResponse>> GetPrinterByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<PrinterResponse>> UpdatePrinterAsync(Guid id, UpdatePrinterRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeletePrinterAsync(Guid id, CancellationToken cancellationToken = default);
}
