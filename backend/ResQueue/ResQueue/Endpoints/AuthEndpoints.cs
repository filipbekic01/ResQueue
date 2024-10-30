using ResQueue.Dtos;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("auth");

        group.MapGet("", (IDbConnectionProvider conn) => Results.Ok(new AuthDto(
            SqlEngine: conn.SqlEngine,
            Username: conn.Username,
            Database: conn.Database,
            Schema: conn.Schema,
            Port: conn.Port
        )));
    }
}