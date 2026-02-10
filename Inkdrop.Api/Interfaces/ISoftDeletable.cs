namespace Inkdrop.Api.Interfaces;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; }
    void MarkAsDeleted();
}