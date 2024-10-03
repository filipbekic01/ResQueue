using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Models;

namespace ResQueue.Features.Messages.CloneMessage;

public record CloneMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, string Id);

public record CloneMessagesFeatureResponse();

public class CloneMessageFeature(
    UserManager<User> userManager,
    IMongoCollection<Message> messagesCollection,
    IMongoCollection<Queue> queuesCollection,
    IMongoClient mongoClient
) : ICloneMessageFeature
{
    public async Task<OperationResult<CloneMessagesFeatureResponse>> ExecuteAsync(CloneMessagesFeatureRequest request)
    {
        // Get user
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<CloneMessagesFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "You must be logged in to sync the broker data.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        // Get message
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(m => m.Id, ObjectId.Parse(request.Id))
        );

        var message = await messagesCollection.Find(filter).SingleOrDefaultAsync();
        if (message == null)
        {
            return OperationResult<CloneMessagesFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Message Not Found",
                Detail = "The message with the specified ID could not be found or does not belong to the current user.",
                Status = StatusCodes.Status404NotFound
            });
        }

        message.Id = default;
        message.UpdatedAt = null;
        message.CreatedAt = DateTime.UtcNow;

        using var session = await mongoClient.StartSessionAsync();
        session.StartTransaction();

        // Insert the message into the messages collection
        await messagesCollection.InsertOneAsync(session, message);

        // Define the update pipeline
        var updatePipeline = new[]
        {
            // First stage: Increment Messages by 1
            new BsonDocument("$set", new BsonDocument
            {
                { "Messages", new BsonDocument("$add", new BsonArray { "$Messages", 1 }) }
            }),
            // Second stage: Update TotalMessages using updated Messages and RawData.messages
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
            x => x.Id == message.QueueId,
            Builders<Queue>.Update.Pipeline(updatePipeline)
        );

        await session.CommitTransactionAsync();

        return OperationResult<CloneMessagesFeatureResponse>.Success(new CloneMessagesFeatureResponse());
    }
}