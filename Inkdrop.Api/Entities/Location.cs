namespace Inkdrop.Api.Entities;

public class Location
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; } = null;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; } = null;
    public DateTime? DeletedAt { get; private set; } = null;
    private Location() { }
    public Location(string name, string? description = null)
    {
        Validate(name);
        Name = name;
        Description = description;
    }
    public void Update(string name, string? description = null)
    {
        Validate(name);
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
    public void MarkAsDeleted()
    {
        if (DeletedAt != null) return;
        DeletedAt = DateTime.UtcNow;
    }
    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name cannot be null or empty.", nameof(name));
        if (name.Length > 100 || name.Length < 3)
            throw new ArgumentException("Location name must be between 3 and 100 characters.", nameof(name));
    }
}