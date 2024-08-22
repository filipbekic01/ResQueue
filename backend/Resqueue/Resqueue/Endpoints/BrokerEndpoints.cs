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

                var filter = Builders<Broker>.Filter.Eq(b => b.UserId, user.Id);
                var brokers = await collection.Find(filter).ToListAsync();
                var dtos = brokers.Select(x => new BrokerDto(
                    x.Id.ToString(),
                    x.System,
                    x.Name,
                    x.Port,
                    x.Host,
                    x.Framework,
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

                if (!string.IsNullOrEmpty(dto.Framework))
                {
                    if (dto.Framework.ToLower() != Frameworks.MASS_TRANSIT)
                    {
                        return Results.BadRequest("Invalid framework.");
                    }
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
                    Framework = string.IsNullOrEmpty(dto.Framework) ? Frameworks.NONE : dto.Framework.ToLower(),
                    CreatedAt = dateTime,
                    UpdatedAt = dateTime
                };

                await collection.InsertOneAsync(broker);

                return Results.Ok();
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

        group.MapPut("/{id}",
            async (string id, [FromBody] UpdateBrokerDto dto, UserManager<User> userManager,
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

                var result = await collection.DeleteOneAsync(filter);

                return result.DeletedCount == 0 ? Results.NotFound() : Results.Ok();
            });

        return group;
    }
}