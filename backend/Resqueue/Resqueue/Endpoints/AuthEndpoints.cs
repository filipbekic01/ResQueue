using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
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
                FullName = user.FullName,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                StripeId = user.StripeId,
                PaymentType = user.PaymentType,
                PaymentLastFour = user.PaymentLastFour,
                Subscriptions = user.Subscriptions.Select(sub => new SubscriptionDto
                {
                    Type = sub.Type,
                    StripeId = sub.StripeId,
                    StripeStatus = sub.StripeStatus,
                    StripePrice = sub.StripePrice,
                    Quantity = sub.Quantity,
                    EndsAt = sub.EndsAt,
                    CreatedAt = sub.CreatedAt,
                    UpdatedAt = sub.UpdatedAt,
                    SubscriptionItems = sub.SubscriptionItems.Select(item => new SubscriptionItemDto
                    {
                        StripeId = item.StripeId,
                        StripeProduct = item.StripeProduct,
                        StripePrice = item.StripePrice,
                        Quantity = item.Quantity,
                        CreatedAt = item.CreatedAt,
                        UpdatedAt = item.UpdatedAt
                    }).ToList()
                }).ToList(),
                UserConfig = new UserConfigDto
                {
                    ShowBrokerSyncConfirm = user.UserConfig.showBrokerSyncConfirm,
                    ShowMessagesSyncConfirm = user.UserConfig.showMessagesSyncConfirm
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

                if (!string.IsNullOrEmpty(dto.PaymentMethodId) && !string.IsNullOrEmpty(dto.Plan))
                {
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

                        // todo: remove from stripe if created by accident

                        return Results.Problem(featureResult.Problem?.Detail,
                            statusCode: featureResult.Problem?.Status ?? 500);
                    }
                }

                return Results.Ok();
            }).AllowAnonymousOnly();

        group.MapPatch("me",
            async (HttpContext httpContext, UserManager<User> userManager, [FromBody] UpdateUserDto dto,
                IMongoCollection<User> usersCollection) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                var update = Builders<User>.Update.Combine();

                // Update FullName if provided
                if (!string.IsNullOrEmpty(dto.FullName))
                {
                    update = Builders<User>.Update.Set(u => u.FullName, dto.FullName);
                }

                // Update UserConfig if provided
                if (dto.Config != null)
                {
                    update = Builders<User>.Update.Combine(
                        update,
                        Builders<User>.Update.Set(u => u.UserConfig.showBrokerSyncConfirm,
                            dto.Config.ShowBrokerSyncConfirm),
                        Builders<User>.Update.Set(u => u.UserConfig.showMessagesSyncConfirm,
                            dto.Config.ShowMessagesSyncConfirm)
                    );
                }

                if (update != Builders<User>.Update.Combine()) // Check if there's any update to apply
                {
                    await usersCollection.UpdateOneAsync(filter, update);
                }

                return Results.Ok();
            }).RequireAuthorization();

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