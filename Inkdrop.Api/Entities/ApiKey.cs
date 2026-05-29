using Inkdrop.Api.Interfaces;

namespace Inkdrop.Api.Entities;

public sealed class ApiKey : Base, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string KeyHash { get; private set; } = string.Empty;
    public DateTime? LastUsedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? DeletedAt { get; private set; } = null;

    private ApiKey() { }

    public ApiKey(string name, string keyHash)
    {
        name = name?.Trim() ?? string.Empty;
        keyHash = keyHash?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name)) AddNotification("ApiKeyNameInvalid", "API Key name cannot be empty.");
        if (string.IsNullOrWhiteSpace(keyHash)) AddNotification("ApiKeyHashInvalid", "API Key hash cannot be empty.");

        if (!IsValid) return;

        Name = name;
        KeyHash = keyHash;
    }

    public void UpdateLastUsed()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateName(string name)
    {
        name = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            AddNotification("ApiKeyNameInvalid", "API Key name cannot be empty.");
            return;
        }

        Name = name;
    }

    public void MarkAsDeleted()
    {
        if (DeletedAt != null) return;
        DeletedAt = DateTime.UtcNow;
    }
}
