using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Dtos.Responses;
using Inkdrop.Api.DTOs.Responses;

namespace Inkdrop.Api.Interfaces;

public interface ITonerService
{
    Task<TonerResponse?> CreateTonerAsync(CreateTonerRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TonerResponse>> GetAllTonersAsync(CancellationToken cancellationToken = default);
    Task<TonerResponse?> GetTonerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TonerResponse?> UpdateTonerAsync(Guid id, UpdateTonerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteTonerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TonerResponse>> GetLowerTonersAsync(int threshold = 3, CancellationToken cancellationToken = default);
}
