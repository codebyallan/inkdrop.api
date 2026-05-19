using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Inkdrop.Api.Services;

public sealed class UserService(ApplicationDbContext dbContext, NotificationContext notificationContext) : IUserService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    public async Task<UserResponse?> CreateUserAsync(RegisterRequest request)
    {
        if (await dbContext.Users.AnyAsync(u => u.Username == request.Username))
            notificationContext.AddNotification("UserUsernameExists", "Username already exists.");

        if (await dbContext.Users.AnyAsync(u => u.Email == request.Email))
            notificationContext.AddNotification("UserEmailExists", "Email already exists.");

        if (!notificationContext.IsValid) return null;

        byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        string salt = Convert.ToBase64String(saltBytes);
        string passwordHash = HashPassword(request.Password, saltBytes);

        User user = new(request.Username, request.Email, passwordHash, salt, request.Role);
        
        if (!user.IsValid)
        {
            notificationContext.AddNotifications(user);
            return null;
        }

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        User? user = await dbContext.Users.FindAsync(id);
        if (user is null) return null;

        string username = request.Username ?? user.Username;
        string email = request.Email ?? user.Email;
        UserRole role = request.Role ?? user.Role;

        if (await dbContext.Users.AnyAsync(u => u.Id != id && u.Username == username))
            notificationContext.AddNotification("UserUsernameExists", "Username already exists.");

        if (await dbContext.Users.AnyAsync(u => u.Id != id && u.Email == email))
            notificationContext.AddNotification("UserEmailExists", "Email already exists.");

        if (!notificationContext.IsValid) return null;

        user.UpdateProfile(username, email, role);
        notificationContext.AddNotifications(user);

        if (!notificationContext.IsValid) return null;

        await dbContext.SaveChangesAsync();
        return MapToResponse(user);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        User? user = await dbContext.Users.FindAsync(id);
        if (user is null) return false;

        user.MarkAsDeleted();
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        User? user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return user is null ? null : MapToResponse(user);
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        return await dbContext.Users.AsNoTracking()
            .Select(u => MapToResponse(u))
            .ToListAsync();
    }

    public async Task<User?> AuthenticateAsync(LoginRequest request)
    {
        User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null) return null;

        if (!user.IsActive)
        {
            notificationContext.AddNotification("UserInactive", "This user account is deactivated.");
            return null;
        }

        byte[] saltBytes = Convert.FromBase64String(user.Salt);
        string computedHash = HashPassword(request.Password, saltBytes);

        if (computedHash != user.PasswordHash)
        {
            notificationContext.AddNotification("InvalidCredentials", "Invalid username or password.");
            return null;
        }

        return user;
    }

    public async Task<bool> ActivateUserAsync(Guid id)
    {
        User? user = await dbContext.Users.FindAsync(id);
        if (user is null) return false;

        user.Activate();
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid id)
    {
        User? user = await dbContext.Users.FindAsync(id);
        if (user is null) return false;

        user.Deactivate();
        await dbContext.SaveChangesAsync();
        return true;
    }

    private string HashPassword(string password, byte[] salt)
    {
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithm,
            HashSize);

        return Convert.ToBase64String(hash);
    }

    private static UserResponse MapToResponse(User user) => 
        new(user.Id, user.Username, user.Email, user.Role.ToString(), user.IsActive, user.CreatedAt);
}
