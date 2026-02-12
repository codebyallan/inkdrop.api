using Inkdrop.Api.Notifications;

namespace Inkdrop.Api.Entities;

public abstract class Base : Notifiable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    protected Base() { }
}
