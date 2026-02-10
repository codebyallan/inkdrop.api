namespace Inkdrop.Api.Entities;

public abstract class Base
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; } = null;
    protected Base() { }
}
