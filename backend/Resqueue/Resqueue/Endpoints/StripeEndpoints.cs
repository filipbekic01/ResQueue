using Microsoft.AspNetCore.Mvc;
using Resqueue.Dtos.Stripe;
using Resqueue.Features.Stripe;
using Resqueue.Features.Stripe.CreateSubscription;
using Resqueue.Features.Stripe.EventHandler;

namespace Resqueue.Endpoints;

public static class StripeEndpoints
{
    public static IEndpointRouteBuilder MapStripeEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("stripe")
            .RequireAuthorization();

        group.MapPost("create-subscription",
            async (HttpContext httpContext, ICreateSubscriptionFeature feature,
                [FromBody] CreateSubscriptionDto dto) =>
            {
                var result = await feature.ExecuteAsync(new CreateSubscriptionRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            });

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