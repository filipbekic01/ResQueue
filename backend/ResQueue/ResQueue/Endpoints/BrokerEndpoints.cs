using Marten;
using Marten.Patching;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResQueue.Constants;
using ResQueue.Dtos;
using ResQueue.Dtos.Broker;
using ResQueue.Enums;
using ResQueue.Features.Broker.AcceptBrokerInvitation;
using ResQueue.Features.Broker.CreateBrokerInvitation;
using ResQueue.Features.Broker.ManageBrokerAccess;
using ResQueue.Features.Broker.UpdateBroker;
using ResQueue.Filters;
using ResQueue.Mappers;
using ResQueue.Models;

namespace ResQueue.Endpoints;

public static class BrokerEndpoints
{
    public static void MapBrokerEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("brokers")
            .RequireAuthorization();

        group.MapGet("",
            async (IDocumentSession documentSession, UserManager<User> userManager, HttpContext httpContext) =>
            {
                // Get user
                var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                // Get brokers
                var brokers = await documentSession.Query<Broker>()
                    .Where(x => x.AccessList.Any(y => y.UserId == user.Id))
                    .Where(x => x.DeletedAt == null)
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                // Filter brokers by permissions
                if (user.Subscription?.Type != StripePlans.ULTIMATE)
                {
                    brokers = brokers.Where(x => x.CreatedByUserId == user.Id).ToList();
                }

                var dtos = brokers.Select(BrokerMapper.ToDto).ToList();

                return Results.Ok(dtos);
            });

        group.MapPost("",
            async (IDocumentSession documentSession, [FromBody] CreateBrokerDto dto,
                UserManager<User> userManager,
                HttpContext httpContext) =>
            {
                // Get user
                var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                if (dto.PostgresConnection is null)
                {
                    return Results.Problem(new ProblemDetails
                    {
                        Title = "Invalid Connection",
                        Detail = "Please provide valid connection details.",
                        Status = StatusCodes.Status403Forbidden
                    });
                }

                // Validate free plan broker count
                if (user.Subscription is null)
                {
                    if (await documentSession.Query<Broker>()
                            .Where(x => x.CreatedByUserId == user.Id)
                            .Where(x => x.DeletedAt == null)
                            .AnyAsync())
                    {
                        return Results.Problem(new ProblemDetails
                        {
                            Title = "Free Plan Limit",
                            Detail = "Upgrade to paid plan to unlock more broker slots.",
                            Status = StatusCodes.Status403Forbidden
                        });
                    }
                }

                var broker = CreateBrokerDtoMapper.ToBroker(user.Id, dto);

                documentSession.Insert(broker);
                await documentSession.SaveChangesAsync();

                return Results.Ok(BrokerMapper.ToDto(broker));
            });

        // group.MapPost("{id}/sync",
        //     async (ISyncBrokerFeature syncBrokerFeature, HttpContext httpContext, string id) =>
        //     {
        //         var result = await syncBrokerFeature.ExecuteAsync(new SyncBrokerFeatureRequest(
        //             ClaimsPrincipal: httpContext.User,
        //             Id: id
        //         ));
        //
        //         return result.IsSuccess
        //             ? Results.Ok(result.Value)
        //             : Results.Problem(result.Problem!);
        //     }); // Do not add retry filter

        group.MapPost("/test-connection",
            async (IHttpClientFactory httpClientFactory, [FromBody] CreateBrokerDto dto) =>
            {
                var broker = CreateBrokerDtoMapper.ToBroker(string.Empty, dto);

                var httpClient = RabbitmqConnectionFactory.CreateManagementClient(httpClientFactory, broker);
                try
                {
                    var response = await httpClient.GetAsync("api/whoami");
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    return Results.Problem(new ProblemDetails
                    {
                        Title = "Connection to Management Endpoint Failed",
                        Detail = $"Unable to connect to the RabbitMQ management endpoint. Error: {ex.Message}",
                        Status = StatusCodes.Status400BadRequest,
                    });
                }

                var factory = RabbitmqConnectionFactory.CreateAmqpFactory(broker);
                try
                {
                    using var connection = factory.CreateConnection();
                    using var channel = connection.CreateModel();
                }
                catch (Exception ex)
                {
                    return Results.Problem(new ProblemDetails
                    {
                        Title = "AMQP Connection Failed",
                        Detail = $"Failed to establish a connection to the RabbitMQ AMQP endpoint. Error: {ex.Message}",
                        Status = StatusCodes.Status400BadRequest,
                    });
                }

                return Results.Ok();
            });

        group.MapPost("/access",
            async ([FromBody] ManageBrokerAccessDto dto,
                HttpContext httpContext, IManageBrokerAccessFeature manageBrokerAccessFeature) =>
            {
                var result = await manageBrokerAccessFeature.ExecuteAsync(new ManageBrokerAccessFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }).AddRetryFilter();

        group.MapGet("/invitations",
            async (HttpContext httpContext, UserManager<User> userManager,
                IDocumentSession documentSession) =>
            {
                var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var invitations = await documentSession.Query<BrokerInvitation>()
                    .Where(x => x.ExpiresAt > DateTime.UtcNow)
                    .Where(x => x.IsAccepted == false)
                    .Where(x => x.InviterId == user.Id)
                    .OrderBy(x => x.CreatedAt)
                    .ToListAsync();

                return Results.Ok(invitations.Select(b => new BrokerInvitationDto
                {
                    Id = b.Id.ToString(),
                    BrokerId = b.BrokerId.ToString(),
                    InviterId = b.InviterId.ToString(),
                    InviteeId = b.InviteeId.ToString(),
                    InviterEmail = b.InviterEmail,
                    Token = b.Token,
                    CreatedAt = b.CreatedAt,
                    ExpiresAt = b.ExpiresAt,
                    IsAccepted = b.IsAccepted,
                    BrokerName = b.BrokerName
                }).ToList());
            }).AddRetryFilter();

        group.MapGet("/invitations/{token}",
            async (string token, IDocumentSession documentSession, UserManager<User> userManager,
                HttpContext httpContext) =>
            {
                var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var brokerInvitation = await documentSession.Query<BrokerInvitation>()
                    .Where(x => x.InviteeId == user.Id)
                    .Where(x => x.Token == token)
                    .FirstOrDefaultAsync();

                if (brokerInvitation == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(new BrokerInvitationDto()
                {
                    Id = brokerInvitation.Id,
                    BrokerId = brokerInvitation.BrokerId,
                    InviterId = brokerInvitation.InviterId,
                    InviteeId = brokerInvitation.InviteeId,
                    InviterEmail = brokerInvitation.InviterEmail,
                    Token = brokerInvitation.Token,
                    CreatedAt = brokerInvitation.CreatedAt,
                    ExpiresAt = brokerInvitation.ExpiresAt,
                    IsAccepted = brokerInvitation.IsAccepted,
                    BrokerName = brokerInvitation.BrokerName
                });
            }).AddRetryFilter();

        group.MapPost("/invitations",
            async ([FromBody] CreateBrokerInvitationDto dto,
                HttpContext httpContext, ICreateBrokerInvitationFeature createBrokerInvitationFeature) =>
            {
                var result = await createBrokerInvitationFeature.ExecuteAsync(new CreateBrokerInvitationRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }).AddRetryFilter();

        group.MapPost("/invitations/accept",
            async (AcceptBrokerInvitationDto dto, HttpContext httpContext,
                IAcceptBrokerInvitationFeature feature) =>
            {
                var result = await feature.ExecuteAsync(new AcceptBrokerInvitationRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }).AddRetryFilter();

        group.MapPost("/invitations/{id}/expire",
            async (HttpContext httpContext, IDocumentSession documentSession,
                UserManager<User> userManager, string id) =>
            {
                var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                documentSession.Patch<BrokerInvitation>(x => x.Id == id && x.InviterId == user.Id)
                    .Set(x => x.ExpiresAt, DateTime.UtcNow);

                return Results.Ok();
            }).AddRetryFilter();

        group.MapPatch("/{id}",
            async (string id, [FromBody] UpdateBrokerDto dto,
                HttpContext httpContext, IUpdateBrokerFeature updateBrokerFeature) =>
            {
                var result = await updateBrokerFeature.ExecuteAsync(new UpdateBrokerFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto,
                    Id: id
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }).AddRetryFilter();

        group.MapDelete("/{id}",
            async (IDocumentSession documentSession, UserManager<User> userManager, HttpContext httpContext,
                string id) =>
            {
                var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                documentSession.Patch<Broker>(x =>
                        x.Id == id && x.AccessList.Any(y => y.UserId == user.Id && y.AccessLevel == AccessLevel.Owner))
                    .Set(x => x.DeletedAt, DateTime.UtcNow);

                return Results.Ok();
            }).AddRetryFilter();
    }
}