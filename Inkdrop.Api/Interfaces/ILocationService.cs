using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;

namespace Inkdrop.Api.Interfaces;

public interface ILocationService
{
    Task<LocationResponse?> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<LocationResponse>> GetAllLocationsAsync(CancellationToken cancellationToken = default);
    Task<LocationResponse?> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LocationResponse?> UpdateLocationAsync(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default);
}
