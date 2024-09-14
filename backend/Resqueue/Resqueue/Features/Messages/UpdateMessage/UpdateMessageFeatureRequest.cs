using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Features.Messages.CreateMessage;
using Resqueue.Mappers;
using Resqueue.Models;

namespace Resqueue.Features.Messages.UpdateMessage;

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
            return OperationResult<UpdateMessageFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "User not found"
            });
        }

        var broker = await brokersCollection.Find(Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Dto.BrokerId)),
            Builders<Models.Broker>.Filter.Eq(b => b.UserId, user.Id)
        )).FirstOrDefaultAsync();

        if (broker == null)
        {
            return OperationResult<UpdateMessageFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "Broker not found"
            });
        }

        var queuesFilter = Builders<Queue>.Filter.And(
            Builders<Queue>.Filter.Eq(b => b.BrokerId, broker.Id),
            Builders<Queue>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Dto.QueueId))
        );

        var queueId = await queuesCollection.Find(queuesFilter).Project(x => x.Id).FirstAsync();

        var message = UpsertMessageDtoMapper.ToMessage(queueId, user.Id, request.Dto);
        message.Id = ObjectId.Parse(request.Id);

        await messagesCollection.ReplaceOneAsync(x => x.Id == message.Id, message);

        return OperationResult<UpdateMessageFeatureResponse>.Success(new UpdateMessageFeatureResponse());
    }
}