using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Constants;
using Resqueue.Dtos;
using Resqueue.Enums;
using Resqueue.Features.Broker.AcceptBrokerInvitation;
using Resqueue.Features.Broker.CreateBrokerInvitation;
using Resqueue.Features.Broker.ManageBrokerAccess;
using Resqueue.Features.Broker.SyncBroker;
using Resqueue.Features.Broker.UpdateBroker;
using Resqueue.Filters;
using Resqueue.Models;

namespace Resqueue.Endpoints;

public static class BrokerEndpoints
{
    public static IEndpointRouteBuilder MapBrokerEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("brokers")
            .RequireAuthorization();

        group.MapGet("",
            async (IMongoCollection<Broker> collection, UserManager<User> userManager, HttpContext httpContext) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.UserId, user.Id),
                    Builders<Broker>.Filter.Eq(b => b.DeletedAt, null));

                var sort = Builders<Broker>.Sort.Descending(b => b.Id);

                var brokers = await collection.Find(filter).Sort(sort).ToListAsync();
                var dtos = brokers.Select(x => new BrokerDto(
                    Id: x.Id.ToString(),
                    System: x.System,
                    Name: x.Name,
                    Port: x.Port,
                    Host: x.Host,
                    VHost: x.VHost,
                    AccessList: x.AccessList.Select(y => new BrokerAccessDto()
                    {
                        UserId = y.UserId.ToString(),
                        AccessLevel = y.AccessLevel
                    }).ToList(),
                    Settings: new BrokerSettingsDto(
                        QuickSearches: x.Settings.QuickSearches,
                        DeadLetterQueueSuffix: x.Settings.DeadLetterQueueSuffix,
                        MessageFormat: x.Settings.MessageFormat,
                        MessageStructure: x.Settings.MessageStructure,
                        QueueTrimPrefix: x.Settings.QueueTrimPrefix,
                        DefaultQueueSortField: x.Settings.DefaultQueueSortField,
                        DefaultQueueSortOrder: x.Settings.DefaultQueueSortOrder,
                        DefaultQueueSearch: x.Settings.DefaultQueueSearch
                    ),
                    CreatedAt: x.CreatedAt,
                    UpdatedAt: x.UpdatedAt,
                    SyncedAt: x.SyncedAt,
                    DeletedAt: x.DeletedAt
                ));

                return Results.Ok(dtos);
            });

        group.MapPost("",
            async (IMongoCollection<Broker> collection, [FromBody] CreateBrokerDto dto, UserManager<User> userManager,
                HttpContext httpContext) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var dateTime = DateTime.UtcNow;

                var broker = new Broker
                {
                    UserId = user.Id,
                    AccessList = new List<BrokerAccess>()
                    {
                        new()
                        {
                            UserId = user.Id,
                            AccessLevel = AccessLevel.Owner
                        }
                    },
                    System = BrokerSystems.RABBIT_MQ,
                    Name = dto.Name,
                    Username = dto.Username,
                    Password = dto.Password,
                    Port = dto.Port,
                    Host = dto.Host,
                    VHost = dto.VHost,
                    CreatedAt = dateTime,
                    UpdatedAt = dateTime
                };

                await collection.InsertOneAsync(broker);

                return Results.Ok(new BrokerDto(
                    Id: broker.Id.ToString(),
                    System: broker.System,
                    Name: broker.Name,
                    Port: broker.Port,
                    Host: broker.Host,
                    VHost: broker.VHost,
                    AccessList: broker.AccessList.Select(y => new BrokerAccessDto()
                    {
                        UserId = y.UserId.ToString(),
                        AccessLevel = y.AccessLevel
                    }).ToList(),
                    Settings: new BrokerSettingsDto(
                        QuickSearches: broker.Settings.QuickSearches,
                        DeadLetterQueueSuffix: broker.Settings.DeadLetterQueueSuffix,
                        MessageFormat: broker.Settings.MessageFormat,
                        MessageStructure: broker.Settings.MessageStructure,
                        QueueTrimPrefix: broker.Settings.QueueTrimPrefix,
                        DefaultQueueSortField: broker.Settings.DefaultQueueSortField,
                        DefaultQueueSortOrder: broker.Settings.DefaultQueueSortOrder,
                        DefaultQueueSearch: broker.Settings.DefaultQueueSearch
                    ),
                    CreatedAt: broker.CreatedAt,
                    UpdatedAt: broker.UpdatedAt,
                    SyncedAt: broker.SyncedAt,
                    DeletedAt: broker.DeletedAt
                ));
            });

        group.MapPost("{id}/sync",
            async (ISyncBrokerFeature syncBrokerFeature, HttpContext httpContext, string id) =>
            {
                var result = await syncBrokerFeature.ExecuteAsync(new SyncBrokerFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Id: id
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).AddRetryFilter();

        group.MapPost("/test-connection",
            async ([FromBody] CreateBrokerDto dto) =>
            {
                using var httpClient = new HttpClient();

                var url = $"https://{dto.Host}:{dto.Port}/api/whoami";

                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{dto.Username}:{dto.Password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        return Results.Ok();
                    }

                    return Results.Problem("Failed to connect to the host.");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Failed to connect to the host: {ex.Message}");
                }
            });

        group.MapPost("/{id}/access",
            async (string id, [FromBody] ManageBrokerAccessDto dto,
                HttpContext httpContext, IManageBrokerAccessFeature manageBrokerAccessFeature) =>
            {
                var result = await manageBrokerAccessFeature.ExecuteAsync(new ManageBrokerAccessFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto,
                    BrokerId: id
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).AddRetryFilter();

        group.MapGet("/invitations/{token}",
            async (string token, IMongoCollection<BrokerInvitation> collection) =>
            {
                var filter = Builders<BrokerInvitation>.Filter.Eq(b => b.Token, token);
                var brokerInvitation = await collection.Find(filter).FirstOrDefaultAsync();

                if (brokerInvitation == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(new BrokerInvitationDto()
                {
                    BrokerId = brokerInvitation.BrokerId.ToString(),
                    UserId = brokerInvitation.UserId.ToString(),
                    Token = brokerInvitation.Token,
                    CreatedAt = brokerInvitation.CreatedAt,
                    ExpiresAt = brokerInvitation.ExpiresAt,
                    IsAccepted = brokerInvitation.IsAccepted,
                });
            }).AddRetryFilter();

        group.MapPost("/invitations",
            async (string id, [FromBody] CreateBrokerInvitationDto dto,
                HttpContext httpContext, ICreateBrokerInvitationFeature createBrokerInvitationFeature) =>
            {
                var result = await createBrokerInvitationFeature.ExecuteAsync(new CreateBrokerInvitationRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto,
                    BrokerId: id
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
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
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
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
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).AddRetryFilter();

        group.MapDelete("{id}",
            async (IMongoCollection<Broker> collection, UserManager<User> userManager, HttpContext httpContext,
                string id) =>
            {
                if (!ObjectId.TryParse(id, out var objectId))
                {
                    return Results.BadRequest("Invalid ID format.");
                }

                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, objectId),
                    Builders<Broker>.Filter.Eq(b => b.UserId, user.Id)
                );

                var update = Builders<Broker>.Update.Set(b => b.DeletedAt, DateTime.UtcNow);

                await collection.UpdateOneAsync(filter, update);

                return Results.Ok();
            });

        return group;
    }
}