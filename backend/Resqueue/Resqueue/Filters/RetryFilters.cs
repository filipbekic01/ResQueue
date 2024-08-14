using MongoDB.Driver;

namespace Resqueue.Filters;

public static class RetryFilterExtensions
{
    public static IEndpointConventionBuilder AddRetryFilter(this IEndpointConventionBuilder builder,
        int maxRetryAttempts = 3, int delayMilliseconds = 50)
    {
        return builder.AddEndpointFilter(async (invocationContext, next) =>
        {
            int retryCount = 0;

            while (retryCount < maxRetryAttempts)
            {
                try
                {
                    return await next(invocationContext);
                }
                catch (MongoCommandException ex) when (ex.Code == 251)
                {
                    retryCount++;
                    if (retryCount >= maxRetryAttempts)
                    {
                        return Results.Problem(
                            "Operation failed after multiple attempts.",
                            statusCode: StatusCodes.Status500InternalServerError
                        );
                    }

                    await Task.Delay(delayMilliseconds);
                }
            }

            return Results.Problem("Unexpected error.", statusCode: StatusCodes.Status500InternalServerError);
        });
    }
}