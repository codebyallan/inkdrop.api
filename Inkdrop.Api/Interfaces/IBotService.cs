using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Core;

namespace Inkdrop.Api.Interfaces;

public interface IBotService
{
    Task<IEnumerable<BotPrinterResponse>> GetMonitoredPrintersAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> SaveTelemetryAsync(TelemetryRequest request, CancellationToken cancellationToken = default);
}
