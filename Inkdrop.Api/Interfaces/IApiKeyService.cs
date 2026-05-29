using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Core;

namespace Inkdrop.Api.Interfaces;

public interface IApiKeyService
{
    Task<ServiceResult<string>> CreateKeyAsync(CreateApiKeyRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiKeyResponse>> GetActiveKeysAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> UpdateKeyNameAsync(Guid id, UpdateApiKeyRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> RevokeKeyAsync(Guid id, CancellationToken cancellationToken = default);
}
