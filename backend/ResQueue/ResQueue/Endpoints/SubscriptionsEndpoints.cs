using ResQueue.Features.Subscriptions.GetSubscriptions;

namespace ResQueue.Endpoints;

public static class SubscriptionsEndpoints
{
    public static void MapSubscriptionsEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("subscriptions");

        group.MapGet("",
            async (IGetSubscriptionsFeature feature) =>
            {
                var result = await feature.ExecuteAsync(new GetSubscriptionsRequest());
                return TypedResults.Ok(result.Subscriptions);
            });
    }
}