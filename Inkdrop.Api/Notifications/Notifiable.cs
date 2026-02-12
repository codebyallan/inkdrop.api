namespace Inkdrop.Api.Notifications;

public abstract class Notifiable
{
    private readonly List<NotificationMessage> _notifications = [];
    public bool IsValid => _notifications.Count == 0;

    public IReadOnlyCollection<NotificationMessage> Notifications => _notifications;

    public void AddNotification(NotificationMessage message)
    {
        if (message is null)
        {
            return;
        }
        _notifications.Add(message);
    }

    public void AddNotification(string key, string message)
    {
        if (key is null || message is null)
        {
            return;
        }
        AddNotification(new NotificationMessage(key, message));
    }

    public void AddNotifications(IReadOnlyCollection<NotificationMessage> notifications)
    {
        if (notifications is null)
        {
            return;
        }
        _notifications.AddRange(notifications);
    }

    public void AddNotifications(Notifiable notifiable)
    {
        if (notifiable is null)
        {
            return;
        }
        _notifications.AddRange(notifiable.Notifications);
    }
    public void ClearNotifications() => _notifications.Clear();
}