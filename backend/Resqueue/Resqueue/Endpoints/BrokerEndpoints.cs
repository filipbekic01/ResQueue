using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Constants;
using Resqueue.Dtos;
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
                    x.Id.ToString(),
                    x.System,
                    x.Name,
                    x.Port,
                    x.Host,
                    new BrokerSettingsDto(
                        QuickSearches: x.Settings.QuickSearches,
                        DeadLetterQueueSuffix: x.Settings.DeadLetterQueueSuffix
                    ),
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.SyncedAt,
                    x.DeletedAt
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
                    System = BrokerSystems.RABBIT_MQ,
                    Name = dto.Name,
                    Username = dto.Username,
                    Password = dto.Password,
                    Port = dto.Port,
                    Host = dto.Host,
                    CreatedAt = dateTime,
                    UpdatedAt = dateTime
                };

                await collection.InsertOneAsync(broker);

                return Results.Ok(new BrokerDto(
                    broker.Id.ToString(),
                    broker.System,
                    broker.Name,
                    broker.Port,
                    broker.Host,
                    new BrokerSettingsDto(
                        QuickSearches: broker.Settings.QuickSearches,
                        DeadLetterQueueSuffix: broker.Settings.DeadLetterQueueSuffix
                    ),
                    broker.CreatedAt,
                    broker.UpdatedAt,
                    broker.SyncedAt,
                    broker.DeletedAt
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