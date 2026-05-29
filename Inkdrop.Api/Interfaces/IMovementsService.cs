using Inkdrop.Api.Core;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Responses;

namespace Inkdrop.Api.Interfaces;

public interface IMovementsService
{
    Task<ServiceResult<MovementsResponse>> CreateAsync(CreateMovementRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<MovementsResponse>> GetAllMovementsAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<MovementsResponse>> GetMovementByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MovementsResponse>> GetMovementsByPrinterIdAsync(Guid printerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MovementsResponse>> GetMovementsByTonerIdAsync(Guid tonerId, CancellationToken cancellationToken = default);
}
