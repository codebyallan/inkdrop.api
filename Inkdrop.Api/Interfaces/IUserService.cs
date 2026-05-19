using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;

namespace Inkdrop.Api.Interfaces;

public interface IUserService
{
    Task<UserResponse?> CreateUserAsync(RegisterRequest request);
    Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
    Task<UserResponse?> GetUserByIdAsync(Guid id);
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<User?> AuthenticateAsync(LoginRequest request);
    Task<bool> ActivateUserAsync(Guid id);
    Task<bool> DeactivateUserAsync(Guid id);
}
