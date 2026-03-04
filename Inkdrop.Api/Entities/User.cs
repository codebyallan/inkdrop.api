using Inkdrop.Api.Interfaces;

namespace Inkdrop.Api.Entities;

class User : Base, ISoftDeletable, IUpdatable
{
    public string Email { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public string Role { get; private set; } = "USER";
    public DateTime? UpdatedAt { get; protected set; } = null;
    public DateTime? DeletedAt { get; private set; } = null;
    private User() { }
    public User(string email, string username, string password)
    {
        email = email?.Trim() ?? string.Empty;
        username = username?.Trim() ?? string.Empty;
        password = password?.Trim() ?? string.Empty;
        Validate(email, username, password);
        if (!IsValid) return;
        Email = email;
        Username = username;
        Password = password;
    }
    public void UpdateEmail(string newEmail)
    {
        newEmail = newEmail?.Trim() ?? string.Empty;
        ValidateEmail(newEmail);
        if (!IsValid) return;
        Email = newEmail;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateUsername(string newUsername)
    {
        newUsername = newUsername?.Trim() ?? string.Empty;
        ValidateUserName(newUsername);
        if (!IsValid) return;
        Username = newUsername;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdatePassword(string newPassword)
    {
        newPassword = newPassword?.Trim() ?? string.Empty;
        ValidatePassword(newPassword);
        if (!IsValid) return;
        Password = newPassword;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateRole(string newRole)
    {
        newRole = newRole?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(newRole)) AddNotification("Role cannot be null or whitespace.", "UserRoleInvalid");
        if (!IsValid) return;
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
    private void ValidateUserName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) AddNotification("Name cannot be null or whitespace.", "UserNameInvalid");
    }
    private void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) AddNotification("Email cannot be null or whitespace.", "UserEmailInvalid");
        else if (!email.Contains('@')) AddNotification("Email must contain '@' character.", "UserEmailInvalid");
    }
    private void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) AddNotification("Password cannot be null or whitespace.", "UserPasswordInvalid");
        else if (password.Length < 6) AddNotification("Password must be at least 6 characters long.", "UserPasswordInvalid");
    }
    private void Validate(string email, string username, string password)
    {
        ValidateEmail(email);
        ValidateUserName(username);
        ValidatePassword(password);
    }
    public void MarkAsDeleted()
    {
        if (DeletedAt != null) return;
        DeletedAt = DateTime.UtcNow;
    }
}