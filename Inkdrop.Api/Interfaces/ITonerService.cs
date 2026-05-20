using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Core;

namespace Inkdrop.Api.Interfaces;

public interface ITonerService
{
    Task<ServiceResult<TonerResponse>> CreateTonerAsync(CreateTonerRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TonerResponse>> GetAllTonersAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<TonerResponse>> GetTonerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<TonerResponse>> UpdateTonerAsync(Guid id, UpdateTonerRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteTonerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TonerResponse>> GetLowerTonersAsync(int threshold = 3, CancellationToken cancellationToken = default);
}
