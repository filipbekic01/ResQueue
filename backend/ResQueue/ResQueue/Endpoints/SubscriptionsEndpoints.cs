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
                var result = await feature.ExecuteAsync(new GetSubscriptionsRequest(
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value!.Subscriptions)
                    : Results.Problem(result.Problem!);
            });
    }
}