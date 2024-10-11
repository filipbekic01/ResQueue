using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Dtos;
using ResQueue.Models;

namespace ResQueue.Features.Messages.ArchiveMessages;

public record ArchiveMessagesFeatureRequest(
    ClaimsPrincipal ClaimsPrincipal,
    ArchiveMessagesDto Dto,
    bool Purge
);

public record ArchiveMessagesFeatureResponse();

public class ArchiveMessagesFeature(
    UserManager<User> userManager,
    IMongoCollection<Message> messagesCollection,
    IMongoCollection<Queue> queuesCollection,
    IMongoClient mongoClient
) : IArchiveMessagesFeature
{
    public async Task<OperationResult<ArchiveMessagesFeatureResponse>> ExecuteAsync(
        ArchiveMessagesFeatureRequest request)
    {
        var dt = DateTime.UtcNow;

        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<ArchiveMessagesFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "You must be logged in to sync the broker data.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var queueId = ObjectId.Parse(request.Dto.QueueId);

        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.In(m => m.Id, request.Dto.Ids.Select(ObjectId.Parse)),
            Builders<Message>.Filter.Eq(m => m.QueueId, queueId)
        );

        var purgeFilter = Builders<Message>.Filter.Eq(m => m.QueueId, queueId);

        var update = Builders<Message>.Update.Set(m => m.DeletedAt, dt);

        using var session = await mongoClient.StartSessionAsync();
        session.StartTransaction();

        // Update many messages in the messages collection
        var result = await messagesCollection.UpdateManyAsync(session, request.Purge ? purgeFilter : filter, update);

        // Define the update pipeline
        var updatePipeline = new[]
        {
            // First stage: Decrement Messages and ensure it's not less than 0
            new BsonDocument("$set", new BsonDocument
            {
                {
                    "Messages", new BsonDocument("$max", new BsonArray
                    {
                        0,
                        new BsonDocument("$subtract", new BsonArray { "$Messages", result.ModifiedCount })
                    })
                }
            }),
            // Second stage: Update TotalMessages using the updated Messages and RawData.messages
            new BsonDocument("$set", new BsonDocument
            {
                {
                    "TotalMessages", new BsonDocument("$max", new BsonArray
                    {
                        0,
                        new BsonDocument("$add", new BsonArray { "$Messages", "$RawData.messages" })
                    })
                }
            })
        };

        // Apply the update pipeline to the queues collection
        await queuesCollection.UpdateOneAsync(
            session,
            x => x.Id == queueId,
            Builders<Queue>.Update.Pipeline(updatePipeline)
        );

        await session.CommitTransactionAsync();

        return OperationResult<ArchiveMessagesFeatureResponse>.Success(new ArchiveMessagesFeatureResponse());
    }
}