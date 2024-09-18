using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Resqueue.Dtos.Stripe;
using Resqueue.Features.Stripe.CancelSubscription;
using Resqueue.Features.Stripe.ChangeCard;
using Resqueue.Features.Stripe.ChangePlan;
using Resqueue.Features.Stripe.ContinueSubscription;
using Resqueue.Features.Stripe.CreateSubscription;
using Resqueue.Features.Stripe.EventHandler;
using Resqueue.Models;

namespace Resqueue.Endpoints;

public static class StripeEndpoints
{
    public static IEndpointRouteBuilder MapStripeEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("stripe");

        group.MapPost("subscribe",
            async (UserManager<User> userManager, [FromBody] SubscribeDto dto, ICreateSubscriptionFeature feature,
                HttpContext httpContext) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                if (string.IsNullOrEmpty(dto.PaymentMethodId) || string.IsNullOrEmpty(dto.Plan))
                {
                    return Results.BadRequest("Invalid plan or payment method");
                }

                var result = await feature.ExecuteAsync(new CreateSubscriptionRequest(
                    UserId: user.Id.ToString(),
                    new(
                        CustomerEmail: user.Email!,
                        PaymentMethodId: dto.PaymentMethodId,
                        Plan: dto.Plan
                    )
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).RequireAuthorization();

        group.MapPost("change-plan",
            async (IChangePlanFeature feature, HttpContext httpContext) =>
            {
                var result = await feature.ExecuteAsync(new ChangePlanRequest(
                    ClaimsPrincipal: httpContext.User
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).RequireAuthorization();

        group.MapPost("change-card",
            async (UserManager<User> userManager, [FromBody] ChangeCardDto dto, IChangeCardFeature feature,
                HttpContext httpContext) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                if (string.IsNullOrEmpty(dto.PaymentMethodId))
                {
                    return Results.BadRequest("Invalid payment method");
                }

                var result = await feature.ExecuteAsync(new ChangeCardRequest(
                    UserId: user.Id.ToString(),
                    new ChangeCardDto(
                        PaymentMethodId: dto.PaymentMethodId
                    )
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).RequireAuthorization();

        group.MapPost("cancel-subscription",
            async (ICancelSubscriptionFeature feature, HttpContext httpContext) =>
            {
                var result = await feature.ExecuteAsync(new CancelSubscriptionRequest(
                    ClaimsPrincipal: httpContext.User
                ));

                return result.IsSuccess
                    ? Results.Ok(new { Message = "Subscription cancelled successfully" })
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).RequireAuthorization();

        group.MapPost("continue-subscription",
            async (IContinueSubscriptionFeature feature, HttpContext httpContext) =>
            {
                var result = await feature.ExecuteAsync(new ContinueSubscriptionRequest(
                    ClaimsPrincipal: httpContext.User
                ));

                return result.IsSuccess
                    ? Results.Ok(new { Message = "Subscription continued successfully" })
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).RequireAuthorization();

        group.MapPost("event-handler", async (HttpContext httpContext, IEventHandlerFeature feature) =>
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