using System.Threading.Channels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Resqueue.Dtos;
using Resqueue.Models;

namespace Resqueue.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("auth")
            .RequireAuthorization();

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
                Email = user.Email,
                UserConfig = new UserConfigDto
                {
                    showBrokerSyncConfirm = user.UserConfig.showBrokerSyncConfirm,
                    showMessagesSyncConfirm = user.UserConfig.showMessagesSyncConfirm
                }
            });
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