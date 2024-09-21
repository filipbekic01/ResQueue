using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ResQueue.Dtos;
using ResQueue.Features.Stripe.CreateSubscription;
using ResQueue.Filters;
using ResQueue.Models;

namespace ResQueue.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
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
                Avatar = user.Avatar,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                StripeId = user.StripeId,
                PaymentType = user.PaymentType,
                PaymentLastFour = user.PaymentLastFour,
                Subscription = user.Subscription != null
                    ? new SubscriptionDto
                    {
                        Type = user.Subscription.Type,
                        StripeId = user.Subscription.StripeId,
                        StripeStatus = user.Subscription.StripeStatus,
                        StripePrice = user.Subscription.StripePrice,
                        Quantity = user.Subscription.Quantity,
                        EndsAt = user.Subscription.EndsAt,
                        CreatedAt = user.Subscription.CreatedAt,
                        UpdatedAt = user.Subscription.UpdatedAt,
                        SubscriptionItem = new SubscriptionItemDto
                        {
                            StripeId = user.Subscription.SubscriptionItem.StripeId,
                            StripeProduct = user.Subscription.SubscriptionItem.StripeProduct,
                            StripePrice = user.Subscription.SubscriptionItem.StripePrice,
                            Quantity = user.Subscription.SubscriptionItem.Quantity,
                            CreatedAt = user.Subscription.SubscriptionItem.CreatedAt,
                            UpdatedAt = user.Subscription.SubscriptionItem.UpdatedAt
                        }
                    }
                    : null,
                Settings = new UserSettingsDto
                {
                    ShowSyncConfirmDialogs = user.Settings.ShowSyncConfirmDialogs,
                    CollapseSidebar = user.Settings.CollapseSidebar
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
                    FullName = DefaultUserNames.GetRandomName(),
                    Avatar = UserAvatarGenerator.GenerateUniqueAvatar(Guid.NewGuid().ToString())
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

        group.MapPatch("me/avatar",
            async (HttpContext httpContext, UserManager<User> userManager,
                IMongoCollection<User> usersCollection) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);

                var update = Builders<User>.Update.Set(u => u.Avatar,
                    UserAvatarGenerator.GenerateUniqueAvatar(user.Id.ToString()));

                await usersCollection.UpdateOneAsync(filter, update);

                return Results.Ok();
            }).RequireAuthorization();

        group.MapPatch("me",
            async
            (HttpContext httpContext, UserManager<User> userManager, [FromBody] UpdateUserDto dto,
                IMongoCollection<User> usersCollection) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                var update = Builders<User>.Update.Combine();

                var trimmed = dto.FullName?.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    update = Builders<User>.Update.Set(u => u.FullName, trimmed);
                }

                // Update UserConfig if provided
                if (dto.Settings != null)
                {
                    update = Builders<User>.Update.Combine(
                        update,
                        Builders<User>.Update.Set(u => u.Settings.ShowSyncConfirmDialogs,
                            dto.Settings.ShowSyncConfirmDialogs),
                        Builders<User>.Update.Set(u => u.Settings.CollapseSidebar,
                            dto.Settings.CollapseSidebar)
                    );
                }

                if (update != Builders<User>.Update.Combine()) // Check if there's any update to apply
                {
                    await usersCollection.UpdateOneAsync(filter, update);
                }

                return Results.Ok();
            }).RequireAuthorization();

        group.MapPost("logout", async
            (SignInManager<User> signInManager,
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
    }
}