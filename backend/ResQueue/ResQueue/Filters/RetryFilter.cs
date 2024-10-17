using Npgsql;

namespace ResQueue.Filters;

public static class RetryFilterExtensions
{
    public static IEndpointConventionBuilder AddRetryFilter(this IEndpointConventionBuilder builder,
        int maxRetryAttempts = 3, int delayMilliseconds = 50)
    {
        return builder.AddEndpointFilter(async (invocationContext, next) =>
        {
            var retryCount = 0;

            while (retryCount < maxRetryAttempts)
            {
                try
                {
                    return await next(invocationContext);
                }
                catch (PostgresException ex) when (ex.IsTransient)
                {
                    retryCount++;
                    if (retryCount >= maxRetryAttempts)
                    {
                        return Results.Problem(
                            title: "Database Operation Failed",
                            detail:
                            $"The operation failed after {maxRetryAttempts} attempts due to a transient database error.",
                            statusCode: StatusCodes.Status500InternalServerError,
                            instance: invocationContext.HttpContext.Request.Path
                        );
                    }

                    await Task.Delay(delayMilliseconds);
                }
            }

            return Results.Problem("Unexpected error.", statusCode: StatusCodes.Status500InternalServerError);
        });
    }
}