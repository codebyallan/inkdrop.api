using Inkdrop.Api.Notifications;

namespace Inkdrop.Api.DTOs.Responses;

public record ErrorResponse(IEnumerable<NotificationMessage> Errors);