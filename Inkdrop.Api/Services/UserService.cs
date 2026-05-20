using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Inkdrop.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace Inkdrop.Api.Services;

public sealed class UserService(ApplicationDbContext dbContext, NotificationContext notificationContext) : IUserService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    public async Task<UserResponse?> CreateUserAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
            notificationContext.AddNotification("UserUsernameExists", "Username already exists.");

        if (await dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            notificationContext.AddNotification("UserEmailExists", "Email already exists.");

        if (!notificationContext.IsValid) return null;

        byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        string salt = Convert.ToBase64String(saltBytes);
        string passwordHash = HashPassword(request.Password, saltBytes);

        User user = new(request.Username, request.Email, passwordHash, salt, request.Role);
        
        user.ValidatePasswordComplexity(request.Password);

        if (!user.IsValid)
        {
            notificationContext.AddNotifications(user);
            return null;
        }

        dbContext.Users.Add(user);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (!DbExceptionHandler.HandleUniqueConstraintViolation(ex, notificationContext)) throw;
            return null;
        }

        return MapToResponse(user);
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null) return null;

        string username = request.Username ?? user.Username;
        string email = request.Email ?? user.Email;
        UserRole role = request.Role ?? user.Role;

        if (await dbContext.Users.AnyAsync(u => u.Id != id && u.Username == username, cancellationToken))
            notificationContext.AddNotification("UserUsernameExists", "Username already exists.");

        if (await dbContext.Users.AnyAsync(u => u.Id != id && u.Email == email, cancellationToken))
            notificationContext.AddNotification("UserEmailExists", "Email already exists.");

        if (!notificationContext.IsValid) return null;

        user.UpdateProfile(username, email, role);
        if (!user.IsValid)
        {
            notificationContext.AddNotifications(user);
        }

        if (!notificationContext.IsValid) return null;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (!DbExceptionHandler.HandleUniqueConstraintViolation(ex, notificationContext)) throw;
            return null;
        }
        return MapToResponse(user);
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        User? user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null) return false;

        user.MarkAsDeleted();
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        User? user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return user is null ? null : MapToResponse(user);
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.AsNoTracking()
            .Select(u => MapToResponse(u))
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
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

    public async Task<bool> ActivateUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        User? user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null) return false;

        user.Activate();
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        User? user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null) return false;

        user.Deactivate();
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null) return false;

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            notificationContext.AddNotification("PasswordRequired", "Both current and new passwords are required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(user.Salt))
        {
            notificationContext.AddNotification("UserAccountError", "User account is missing security salt. Please contact an administrator.");
            return false;
        }

        try
        {
            byte[] saltBytes = Convert.FromBase64String(user.Salt);
            string computedHash = HashPassword(request.CurrentPassword, saltBytes);

            if (computedHash != user.PasswordHash)
            {
                notificationContext.AddNotification("InvalidCurrentPassword", "The current password provided is incorrect.");
                return false;
            }
        }
        catch (FormatException)
        {
            notificationContext.AddNotification("UserAccountError", "User account security data is corrupted. Please contact an administrator.");
            return false;
        }

        if (request.NewPassword == request.CurrentPassword)
        {
            notificationContext.AddNotification("NewPasswordSameAsOld", "The new password cannot be the same as the current password.");
            return false;
        }

        user.ValidatePasswordComplexity(request.NewPassword);
        if (!user.IsValid)
        {
            notificationContext.AddNotifications(user);
            return false;
        }

        byte[] newSaltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        string newSalt = Convert.ToBase64String(newSaltBytes);
        string newHash = HashPassword(request.NewPassword, newSaltBytes);

        user.UpdatePassword(newHash, newSalt);
        await dbContext.SaveChangesAsync(cancellationToken);
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
