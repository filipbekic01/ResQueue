using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Resqueue.Dtos;
using Resqueue.Features.Stripe.CreateSubscription;
using Resqueue.Filters;
using Resqueue.Models;

namespace Resqueue.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("auth");

        group.MapGet("me", async (HttpContext httpContext, UserManager<User> userManager) =>
        {
            var user = await userManager.GetUserAsync(httpContext.User);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new UserDto()
            {
                Id = user.Id.ToString(),
                Email = user.Email!,
                SubscriptionId = user.SubscriptionId,
                IsSubscribed = user.IsSubscribed,
                SubscriptionPlan = user.SubscriptionPlan,
                UserConfig = new UserConfigDto
                {
                    showBrokerSyncConfirm = user.UserConfig.showBrokerSyncConfirm,
                    showMessagesSyncConfirm = user.UserConfig.showMessagesSyncConfirm
                }
            });
        }).RequireAuthorization();

        group.MapPost("register",
            async (UserManager<User> userManager, [FromBody] RegisterDto dto, ICreateSubscriptionFeature feature) =>
            {
                if (!string.IsNullOrWhiteSpace(dto.Email) && !new EmailAddressAttribute().IsValid(dto.Email))
                {
                    return Results.Problem("Invalid e-mails address.");
                }

                var user = new User
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                };

                var result = await userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Results.Problem(errors);
                }

                var featureResult = await feature.ExecuteAsync(new CreateSubscriptionRequest(
                    UserId: user.Id.ToString(),
                    new(
                        CustomerEmail: user.Email,
                        PaymentMethodId: dto.PaymentMethodId,
                        Plan: dto.Plan
                    )
                ));

                if (!featureResult.IsSuccess)
                {
                    await userManager.DeleteAsync(user);

                    return Results.Problem(featureResult.Problem?.Detail,
                        statusCode: featureResult.Problem?.Status ?? 500);
                }

                return Results.Ok();
            }).AllowAnonymousOnly();

        group.MapPost("logout", async (SignInManager<User> signInManager,
                [FromBody] object empty) =>
            {
                if (empty != null)
                {
                    await signInManager.SignOutAsync();
                    return Results.Ok();
                }

                return Results.Unauthorized();
            })
            .RequireAuthorization();

        return group;
    }
}