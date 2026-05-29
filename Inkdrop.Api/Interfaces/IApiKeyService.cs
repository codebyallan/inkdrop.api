using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Core;

namespace Inkdrop.Api.Interfaces;

public interface IApiKeyService
{
    Task<ServiceResult<string>> CreateKeyAsync(CreateApiKeyRequest request);
    Task<IEnumerable<ApiKeyResponse>> GetActiveKeysAsync();
    Task<ServiceResult<bool>> UpdateKeyNameAsync(Guid id, UpdateApiKeyRequest request);
    Task<ServiceResult<bool>> RevokeKeyAsync(Guid id);
}
