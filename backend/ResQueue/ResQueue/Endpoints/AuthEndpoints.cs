using Microsoft.Extensions.Options;
using ResQueue.Dtos;

namespace ResQueue.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("auth");

        group.MapGet("", (IOptions<ResQueueOptions> options) => Results.Ok(new AuthDto(
            SqlEngine: options.Value.SqlEngine,
            Username: options.Value.Username,
            Database: options.Value.Database,
            Schema: options.Value.Schema,
            Port: options.Value.Port
        )));
    }
}