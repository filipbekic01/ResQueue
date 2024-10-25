namespace ResQueue.Endpoints;

public static class AuthEndpoints
{
    private static readonly IResult OkResult = Results.Ok();

    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("auth");

        group.MapGet("", () => OkResult);
    }
}