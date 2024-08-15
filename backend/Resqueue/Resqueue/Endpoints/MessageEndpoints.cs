using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Features.Messages.ArchiveMessages;
using Resqueue.Features.Messages.PublishMessages;
using Resqueue.Features.Messages.SyncMessages;
using Resqueue.Filters;
using Resqueue.Models;

namespace Resqueue.Endpoints;

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("messages")
            .RequireAuthorization();

        group.MapGet("{id}",
            async (IMongoCollection<Message> messagesCollection, UserManager<User> userManager, HttpContext httpContext,
                string id) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                if (!ObjectId.TryParse(id, out var queueId))
                {
                    return Results.BadRequest("Invalid Broker ID format.");
                }

                var filter = Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(q => q.QueueId, queueId)
                );

                var messages = await messagesCollection.Find(filter).ToListAsync();

                return Results.Ok(messages.Select(q => new MessageDto()
                {
                    Id = q.Id.ToString(),
                    RawData = q.RawData.ToString()
                }));
            });

        group.MapPost("{id}/sync",
            async (ISyncMessagesFeature syncMessagesFeature, HttpContext httpContext, string id) =>
            {
                var result = await syncMessagesFeature.ExecuteAsync(new SyncMessagesFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Id: id
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).AddRetryFilter();

        group.MapPost("publish",
            async (IPublishMessagesFeature publishMessagesFeature, HttpContext httpContext,
                [FromBody] PublishDto dto) =>
            {
                var result = await publishMessagesFeature.ExecuteAsync(new PublishMessagesFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).AddRetryFilter();

        group.MapDelete("{id}",
            async (IArchiveMessagesFeature archiveMessagesFeature, ArchiveMessagesDto dto, HttpContext httpContext,
                string id) =>
            {
                var result = await archiveMessagesFeature.ExecuteAsync(new ArchiveMessagesFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    QueueId: id,
                    Dto: dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem?.Detail, statusCode: result.Problem?.Status ?? 500);
            }).AddRetryFilter();

        return group;
    }
}