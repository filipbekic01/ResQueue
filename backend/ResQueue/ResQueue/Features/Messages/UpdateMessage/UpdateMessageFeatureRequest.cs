using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Dtos;
using ResQueue.Mappers;
using ResQueue.Models;

namespace ResQueue.Features.Messages.UpdateMessage;

public record UpdateMessageFeatureRequest(
    ClaimsPrincipal ClaimsPrincipal,
    string Id,
    UpsertMessageDto Dto
);

public record UpdateMessageFeatureResponse();

public class UpdateMessageFeature(
    IMongoCollection<Models.Broker> brokersCollection,
    IMongoCollection<Queue> queuesCollection,
    IMongoCollection<Message> messagesCollection,
    UserManager<User> userManager
) : IUpdateMessageFeature
{
    public async Task<OperationResult<UpdateMessageFeatureResponse>> ExecuteAsync(
        UpdateMessageFeatureRequest request)
    {
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<UpdateMessageFeatureResponse>.Failure(new ProblemDetails
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

        var messageId = ObjectId.Parse(request.Id);

        var originalMessage = await messagesCollection.Find(x => x.Id == messageId).FirstAsync();

        var message = UpsertMessageDtoMapper.ToMessage(queue.Id, user.Id, queue.NextMessageOrder, request.Dto);

        message.Id = messageId;
        message.CreatedAt = originalMessage.CreatedAt;
        message.UpdatedAt = DateTime.UtcNow;

        if (message.RabbitMQMeta is not null)
        {
            message.RabbitMQMeta.Exchange = originalMessage.RabbitMQMeta?.Exchange;
        }

        await messagesCollection.ReplaceOneAsync(x => x.Id == message.Id, message);

        return OperationResult<UpdateMessageFeatureResponse>.Success(new UpdateMessageFeatureResponse());
    }
}