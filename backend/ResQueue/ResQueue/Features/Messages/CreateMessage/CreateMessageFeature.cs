using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Dtos;
using ResQueue.Mappers;
using ResQueue.Models;

namespace ResQueue.Features.Messages.CreateMessage;

public record CreateMessageFeatureRequest(
    ClaimsPrincipal ClaimsPrincipal,
    UpsertMessageDto Dto
);

public record CreateMessageFeatureResponse();

public class CreateMessageFeature(
    IMongoClient mongoClient,
    IMongoCollection<Models.Broker> brokersCollection,
    IMongoCollection<Queue> queuesCollection,
    IMongoCollection<Message> messagesCollection,
    UserManager<User> userManager
) : ICreateMessageFeature
{
    public async Task<OperationResult<CreateMessageFeatureResponse>> ExecuteAsync(
        CreateMessageFeatureRequest request)
    {
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<CreateMessageFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "The user could not be found or is not logged in.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var broker = await brokersCollection.Find(Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Dto.BrokerId)),
            Builders<Models.Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id)
        )).SingleAsync();

        var queuesFilter = Builders<Queue>.Filter.And(
            Builders<Queue>.Filter.Eq(b => b.BrokerId, broker.Id),
            Builders<Queue>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Dto.QueueId))
        );

        var queue = await queuesCollection.FindOneAndUpdateAsync(queuesFilter,
            Builders<Queue>.Update.Inc(x => x.NextMessageOrder, 1));
        if (queue is null)
        {
            return OperationResult<CreateMessageFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Queue Not Found",
                Detail = "The specified queue could not be found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var message = UpsertMessageDtoMapper.ToMessage(queue.Id, user.Id, queue.NextMessageOrder, request.Dto);

        using var session = await mongoClient.StartSessionAsync();
        session.StartTransaction();

        // Insert the message into the messages collection
        await messagesCollection.InsertOneAsync(session, message);

        // Define the update pipeline with multiple stages
        var updatePipeline = new[]
        {
            // First stage: Increment Messages by 1
            new BsonDocument("$set", new BsonDocument
            {
                { "Messages", new BsonDocument("$add", new BsonArray { "$Messages", 1 }) }
            }),
            // Second stage: Update TotalMessages using the updated value of Messages
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

        // Apply the update pipeline to the queue
        await queuesCollection.UpdateOneAsync(
            session,
            x => x.Id == message.QueueId,
            Builders<Queue>.Update.Pipeline(updatePipeline)
        );

        await session.CommitTransactionAsync();

        return OperationResult<CreateMessageFeatureResponse>.Success(new CreateMessageFeatureResponse());
    }
}