using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;

namespace Inkdrop.Api.Interfaces;

public interface IUserService
{
    Task<UserResponse?> CreateUserAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<User?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<bool> ActivateUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(Guid id, CancellationToken cancellationToken = default);
}
