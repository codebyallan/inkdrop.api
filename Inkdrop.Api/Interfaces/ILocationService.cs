using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Core;

namespace Inkdrop.Api.Interfaces;

public interface ILocationService
{
    Task<ServiceResult<LocationResponse>> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<LocationResponse>> GetAllLocationsAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<LocationResponse>> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<LocationResponse>> UpdateLocationAsync(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default);
}
