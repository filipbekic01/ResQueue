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

        await messagesCollection.InsertOneAsync(session, message);
        await queuesCollection.UpdateOneAsync(session, x => x.Id == queue.Id,
            Builders<Queue>.Update.Inc(x => x.TotalMessages, 1));

        await session.CommitTransactionAsync();

        return OperationResult<CreateMessageFeatureResponse>.Success(new CreateMessageFeatureResponse());
    }
}