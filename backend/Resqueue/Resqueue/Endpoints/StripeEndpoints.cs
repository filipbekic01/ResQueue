using Resqueue.Features.Stripe.EventHandler;

namespace Resqueue.Endpoints;

public static class StripeEndpoints
{
    public static IEndpointRouteBuilder MapStripeEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("stripe")
            .RequireAuthorization();

        group.MapGet("event-handler", async (HttpContext httpContext, IEventHandlerFeature feature) =>
        {
            var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();

            if (!httpContext.Request.Headers.TryGetValue("Stripe-Signature", out var signature))
            {
                return Results.Problem("Invalid signature header");
            }

            var result = await feature.ExecuteAsync(new EventHandlerRequest(
                JsonBody: json,
                Signature: signature.ToString()
            ));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
        });

        return group;
    }
}