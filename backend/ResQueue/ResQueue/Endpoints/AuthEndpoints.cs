using System.ComponentModel.DataAnnotations;
using Marten;
using Marten.Patching;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResQueue.Dtos;
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
            var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new UserDto()
            {
                Id = user.Id,
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
            async (UserManager<User> userManager, [FromBody] RegisterDto dto) =>
            {
                if (!string.IsNullOrWhiteSpace(dto.Email) && !new EmailAddressAttribute().IsValid(dto.Email))
                {
                    return Results.Problem(new ProblemDetails()
                    {
                        Title = "Invalid e-mails address.",
                        Detail = "Enter valid e-mail address to continue",
                    });
                }

                var user = new User
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = DefaultUserNames.GetRandomName(),
                    Avatar = UserAvatarGenerator.GenerateUniqueAvatar(Guid.NewGuid().ToString()),
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Results.Problem(errors);
                }

                // if (!string.IsNullOrEmpty(dto.PaymentMethodId) && !string.IsNullOrEmpty(dto.Plan))
                // {
                //     var featureResult = await feature.ExecuteAsync(new CreateSubscriptionRequest(
                //         UserId: user.Id.ToString(),
                //         new CreateSubscriptionDto(
                //             CustomerEmail: user.Email,
                //             PaymentMethodId: dto.PaymentMethodId,
                //             Plan: dto.Plan,
                //             Coupon: dto.Coupon
                //         )
                //     ));
                //
                //     if (!featureResult.IsSuccess)
                //     {
                //         await userManager.DeleteAsync(user);
                //
                //         return Results.Problem(featureResult.Problem!);
                //     }
                // }

                return Results.Ok();
            }).AllowAnonymousOnly();

        group.MapPatch("me/avatar",
            async (HttpContext httpContext, UserManager<User> userManager, IDocumentSession documentSession) =>
            {
                // Get the current user
                var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                // Load the user document by id
                var existingUser = await documentSession.LoadAsync<User>(user.Id);
                if (existingUser == null)
                {
                    return Results.NotFound();
                }

                // Update the avatar for the user
                var newAvatar = UserAvatarGenerator.GenerateUniqueAvatar(user.Id);
                if (string.IsNullOrEmpty(newAvatar))
                {
                    return Results.Problem(new ProblemDetails()
                    {
                        Title = "Invalid avatar generated.",
                        Detail = "Something went wrong with avatar generator.",
                    });
                }

                documentSession.Patch<User>(user.Id).Set(x => x.Avatar, newAvatar);

                await documentSession.SaveChangesAsync();

                return Results.Ok();
            }).RequireAuthorization();

        group.MapPatch("me",
            async
            (HttpContext httpContext, UserManager<User> userManager, [FromBody] UpdateUserDto dto,
                IDocumentSession documentSession) =>
            {
                var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var patch = documentSession.Patch<User>(user.Id);

                var trimmed = dto.FullName?.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    patch.Set(x => x.FullName, trimmed);
                }

                // Update UserConfig if provided
                if (dto.Settings != null)
                {
                    patch.Set(x => x.Settings.ShowSyncConfirmDialogs, dto.Settings.ShowSyncConfirmDialogs);
                    patch.Set(x => x.Settings.CollapseSidebar, dto.Settings.CollapseSidebar);
                }

                await documentSession.SaveChangesAsync();

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