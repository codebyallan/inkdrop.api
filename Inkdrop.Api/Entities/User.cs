using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Inkdrop.Api.Entities;

public sealed class User : Base, ISoftDeletable, IUpdatable
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Salt { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? UpdatedAt { get; private set; } = null;
    public DateTime? DeletedAt { get; private set; } = null;
    [Timestamp]
    public byte[] RowVersion { get; private set; } = null!;

    private User() { }

    public User(string username, string email, string passwordHash, string salt, UserRole? role)
    {
        username = username?.Trim() ?? string.Empty;
        email = email?.Trim() ?? string.Empty;
        passwordHash = passwordHash?.Trim() ?? string.Empty;
        salt = salt?.Trim() ?? string.Empty;

        Validate(username, email, passwordHash, salt);
        
        if (!IsValid) return;

        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Salt = salt;
        Role = role ?? UserRole.Technician;
    }

    public void UpdateProfile(string username, string email, UserRole role)
    {
        username = username?.Trim() ?? string.Empty;
        email = email?.Trim() ?? string.Empty;

        if (Username == username && Email == email && Role == role) return;

        ValidateUsername(username);
        ValidateEmail(email);

        if (!IsValid) return;

        Username = username;
        Email = email;
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newHash, string newSalt)
    {
        if (string.IsNullOrWhiteSpace(newHash) || string.IsNullOrWhiteSpace(newSalt))
        {
            AddNotification("UserPasswordInvalid", "Password hash and salt cannot be empty.");
            return;
        }

        PasswordHash = newHash;
        Salt = newSalt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ValidatePasswordComplexity(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            AddNotification("PasswordRequired", "Password is required.");
            return;
        }

        if (password.Length < 6) AddNotification("PasswordTooShort", "Password must be at least 6 characters long.");
        if (!password.Any(char.IsLower)) AddNotification("PasswordMissingLowercase", "Password must contain at least one lowercase letter.");
        if (!password.Any(char.IsUpper)) AddNotification("PasswordMissingUppercase", "Password must contain at least one uppercase letter.");
        if (!password.Any(char.IsDigit)) AddNotification("PasswordMissingDigit", "Password must contain at least one number.");
        if (!password.Any(ch => !char.IsLetterOrDigit(ch))) AddNotification("PasswordMissingSpecialChar", "Password must contain at least one special character.");
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (DeletedAt != null) return;
        DeletedAt = DateTime.UtcNow;
    }

    private void Validate(string username, string email, string passwordHash, string salt)
    {
        ValidateUsername(username);
        ValidateEmail(email);

        if (string.IsNullOrWhiteSpace(passwordHash)) AddNotification("UserPasswordHashInvalid", "Password hash is required.");
        if (string.IsNullOrWhiteSpace(salt)) AddNotification("UserSaltInvalid", "Salt is required.");
    }

    private void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            AddNotification("UserUsernameInvalid", "Username cannot be null or empty.");
            return;
        }
        if (username.Length < 3 || username.Length > 50) AddNotification("UserUsernameLengthInvalid", "Username must be between 3 and 50 characters.");
        if (!InputValidator.IsSafe(username)) AddNotification("UserUsernameUnsafe", "Username contains forbidden characters or patterns.");
    }

    private void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            AddNotification("UserEmailInvalid", "Email cannot be null or empty.");
            return;
        }
        if (!InputValidator.IsSafe(email)) AddNotification("UserEmailUnsafe", "Email contains forbidden characters or patterns.");
        try { _ = new System.Net.Mail.MailAddress(email); }
        catch (FormatException)
        {
            AddNotification("UserEmailFormatInvalid", "Invalid email format.");
        }
    }
}
