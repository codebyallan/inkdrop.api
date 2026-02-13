using Inkdrop.Api.Interfaces;

namespace Inkdrop.Api.Entities;

public class Location : Base, ISoftDeletable, IUpdatable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; } = null;
    public DateTime? UpdatedAt { get; protected set; } = null;
    public DateTime? DeletedAt { get; private set; } = null;
    private Location() { }
    public Location(string name, string? description = null)
    {
        name = name?.Trim() ?? string.Empty;
        description = description?.Trim() ?? null;
        Validate(name);
        Name = name;
        Description = description;
    }
    public void Update(string name, string? description = null)
    {
        name = name?.Trim() ?? string.Empty;
        description = description?.Trim() ?? null;
        if (Name == name && Description == description) return;
        Validate(name);
        if (!IsValid) return;
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
    public void MarkAsDeleted()
    {
        if (DeletedAt != null) return;
        DeletedAt = DateTime.UtcNow;
    }
    private void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AddNotification("LocationNameInvalid", "Location name cannot be null or empty.");
            return;
        }
        if (name.Length > 100 || name.Length < 3)
            AddNotification("LocationNameLengthInvalid", "Location name must be between 3 and 100 characters.");
    }
}