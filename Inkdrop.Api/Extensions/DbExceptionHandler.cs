using Inkdrop.Api.Notifications;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Inkdrop.Api.Extensions;

public static class DbExceptionHandler
{
    public static bool HandleUniqueConstraintViolation(DbUpdateException ex, NotificationContext notificationContext)
    {
        if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            string constraintName = pgEx.ConstraintName ?? string.Empty;

            if (constraintName.Contains("Username", StringComparison.OrdinalIgnoreCase))
            {
                notificationContext.AddNotification("UserUsernameExists", "Username already exists.");
                return true;
            }
            
            if (constraintName.Contains("Email", StringComparison.OrdinalIgnoreCase))
            {
                notificationContext.AddNotification("UserEmailExists", "Email already exists.");
                return true;
            }

            if (constraintName.Contains("Location_Name", StringComparison.OrdinalIgnoreCase) || constraintName.Contains("Location") && constraintName.Contains("Name"))
            {
                notificationContext.AddNotification("LocationNameExists", "Location name already exists.");
                return true;
            }

            if (constraintName.Contains("Printer_IpAddress", StringComparison.OrdinalIgnoreCase) || constraintName.Contains("Printer") && constraintName.Contains("IpAddress"))
            {
                notificationContext.AddNotification("PrinterIpAddressExists", "Printer IP address already exists.");
                return true;
            }

            if (constraintName.Contains("Toner") && (constraintName.Contains("Model") || constraintName.Contains("Manufacturer") || constraintName.Contains("Color")))
            {
                notificationContext.AddNotification("TonerAlreadyExists", "A toner with this model, manufacturer and color already exists.");
                return true;
            }
        }

        return false;
    }

    public static bool HandleConcurrencyException(DbUpdateConcurrencyException ex, NotificationContext notificationContext)
    {
        notificationContext.AddNotification("ConcurrencyConflict", "The record you are attempting to update was modified by another user. Please reload the data and try again.");
        return true;
    }
}
