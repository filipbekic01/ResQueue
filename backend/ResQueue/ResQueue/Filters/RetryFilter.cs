using MongoDB.Driver;

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
                catch (MongoException ex) when (IsTransientError(ex))
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

    private static readonly int[] MongoDbTransientErrorCodes =
    [
        6, // HostUnreachable
        7, // HostNotFound
        89, // NetworkTimeout
        91, // ShutdownInProgress
        189, // PrimarySteppedDown
        262, // ExceededTimeLimit
        9001, // SocketException
        10107, // NotMaster
        11600, // InterruptedAtShutdown
        11602, // InterruptedDueToReplStateChange
        13435, // NotMasterNoSlaveOk
        13436, // NotMasterOrSecondary
        251, // NoSuchTransaction
        112 // WriteConflict
    ];

    private static bool IsTransientError(MongoException ex) => ex switch
    {
        MongoCommandException cmdEx => MongoDbTransientErrorCodes.Contains(cmdEx.Code),
        MongoConnectionException or MongoNotPrimaryException or MongoNodeIsRecoveringException => true,
        _ => false
    };
}