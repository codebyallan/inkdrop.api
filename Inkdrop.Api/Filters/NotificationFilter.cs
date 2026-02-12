using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Inkdrop.Api.Notifications;

namespace Inkdrop.Api.Filters;

public class NotificationFilter(NotificationContext notificationContext) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!notificationContext.IsValid) context.Result = new BadRequestObjectResult(new { Errors = notificationContext.Notifications });
        await next();
    }
}