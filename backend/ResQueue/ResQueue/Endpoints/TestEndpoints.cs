namespace ResQueue.Endpoints;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("test");

        group.MapGet("get-500", () => { throw new Exception("get500exception"); });

        group.MapPost("post-500", () => { throw new Exception("post500exception"); });
    }
}