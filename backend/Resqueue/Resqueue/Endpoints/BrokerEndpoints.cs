using System.Net.Http.Headers;
using System.Text;
using Amazon.Runtime;
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
using Resqueue.Mappers;
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
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
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
                    return Results.Problem(new ProblemDetails()
                    {
                        Title = $"Test Failed to connect to the management endpoint: {ex.Message}",
                        Status = 400,
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
                    return Results.Problem(new ProblemDetails()
                    {
                        Title = $"Failed to connect to the amqp endpoint: {ex.Message}",
                        Status = 400,
                    });
                }

                return Results.Ok();
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