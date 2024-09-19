namespace ResQueue.Filters;

public static class AnonymousOnlyFilter
{
    public static IEndpointConventionBuilder AllowAnonymousOnly(this IEndpointConventionBuilder builder)
    {
        return builder.AddEndpointFilter(async (invocationContext, next) =>
        {
            if (invocationContext.HttpContext.User.Identity?.IsAuthenticated is true)
            {
                return Results.Problem(
                    "Authorized clients can't call this endpoint.",
                    statusCode: StatusCodes.Status403Forbidden
                );
            }

            return await next(invocationContext);
        });
    }
}