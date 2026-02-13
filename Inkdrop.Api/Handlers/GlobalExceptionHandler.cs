using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Inkdrop.Api.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled error detected: {Message}", exception.Message);
        ProblemDetails problemDetails = new()
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Internal Server Error!",
            Detail = "An unexpected error occurred. Our technical team has already been notified.",
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}