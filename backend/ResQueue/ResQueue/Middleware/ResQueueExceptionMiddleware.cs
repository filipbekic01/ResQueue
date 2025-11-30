using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ResQueue.Exceptions;

namespace ResQueue.Middleware;

public class ResQueueExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ResQueueException ex)
        {
            await HandleResQueueExceptionAsync(context, ex);
        }
    }

    private static async Task HandleResQueueExceptionAsync(HttpContext context, ResQueueException exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = exception.StatusCode;

        var problemDetails = new ProblemDetails
        {
            Status = exception.StatusCode,
            Title = exception.Message,
            Detail = exception.Detail
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
