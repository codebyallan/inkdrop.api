using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Core;

namespace Inkdrop.Api.Interfaces;

public interface IUserService
{
    Task<ServiceResult<UserResponse>> CreateUserAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserResponse>> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserResponse>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<AuthResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> ActivateUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeactivateUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> ChangePasswordAsync(Guid id, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> ResetPasswordAsync(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
