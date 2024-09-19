using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Dtos.Broker;
using Resqueue.Features.Broker.AcceptBrokerInvitation;
using Resqueue.Features.Broker.CreateBrokerInvitation;
using Resqueue.Features.Broker.ManageBrokerAccess;
using Resqueue.Features.Broker.SyncBroker;
using Resqueue.Features.Broker.UpdateBroker;
using Resqueue.Filters;
using Resqueue.Mappers;
using Resqueue.Models;

namespace Resqueue.Endpoints;

public static class BrokerEndpoints
{
    public static void MapBrokerEndpoints(this IEndpointRouteBuilder routes)
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
                    Builders<Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id),
                    Builders<Broker>.Filter.Eq(b => b.DeletedAt, null));

                var sort = Builders<Broker>.Sort.Descending(b => b.Id);

                var brokers = await collection.Find(filter).Sort(sort).ToListAsync();
                var dtos = brokers.Select(BrokerMapper.ToDto).ToList();

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

                var broker = CreateBrokerDtoMapper.ToBroker(user.Id, dto);

                await collection.InsertOneAsync(broker);

                return Results.Ok(BrokerMapper.ToDto(broker));
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
                    : Results.Problem(result.Problem!);
            }).AddRetryFilter();

        group.MapPost("/test-connection",
            async (IHttpClientFactory httpClientFactory, [FromBody] CreateBrokerDto dto) =>
            {
                var broker = CreateBrokerDtoMapper.ToBroker(ObjectId.Empty, dto);

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
            async (HttpContext httpContext, UserManager<User> userManager, [FromQuery] string? brokerId,
                IMongoCollection<BrokerInvitation> collection) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filterList = new List<FilterDefinition<BrokerInvitation>>();

                filterList.Add(Builders<BrokerInvitation>.Filter.Gt(b => b.ExpiresAt, DateTime.UtcNow));
                filterList.Add(Builders<BrokerInvitation>.Filter.Eq(b => b.IsAccepted, false));

                if (!string.IsNullOrEmpty(brokerId))
                {
                    // todo: make sure broker belongs to a user
                    filterList.Add(Builders<BrokerInvitation>.Filter.Eq(b => b.BrokerId, ObjectId.Parse(brokerId)));
                }
                else
                {
                    filterList.Add(Builders<BrokerInvitation>.Filter.Eq(b => b.InviterId, user.Id));
                }

                var filter = Builders<BrokerInvitation>.Filter.And(filterList);

                var sort = Builders<BrokerInvitation>.Sort.Descending(b => b.CreatedAt);

                var invitations = await collection
                    .Find(filter)
                    .Sort(sort)
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
                    Id = brokerInvitation.Id.ToString(),
                    BrokerId = brokerInvitation.BrokerId.ToString(),
                    InviterId = brokerInvitation.InviterId.ToString(),
                    InviteeId = brokerInvitation.InviteeId.ToString(),
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
            async (HttpContext httpContext, IMongoCollection<BrokerInvitation> collection,
                UserManager<User> userManager, string id) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<BrokerInvitation>.Filter.And(
                    Builders<BrokerInvitation>.Filter.Eq(b => b.Id, ObjectId.Parse(id)),
                    Builders<BrokerInvitation>.Filter.Eq(b => b.InviterId, user.Id)
                );

                var update = Builders<BrokerInvitation>.Update
                    .Set(b => b.ExpiresAt, DateTime.UtcNow);

                await collection.UpdateOneAsync(filter, update);

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
                    Builders<Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id)
                );

                var update = Builders<Broker>.Update.Set(b => b.DeletedAt, DateTime.UtcNow);

                await collection.UpdateOneAsync(filter, update);

                return Results.Ok();
            });
    }
}