using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Mappers;
using Resqueue.Models;

namespace Resqueue.Features.Messages.CreateMessage;

public record CreateMessageFeatureRequest(
    ClaimsPrincipal ClaimsPrincipal,
    CreateMessageDto Dto
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
            return OperationResult<CreateMessageFeatureResponse>.Failure(new ProblemDetails()
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
            return OperationResult<CreateMessageFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "Broker not found"
            });
        }

        var queuesFilter = Builders<Queue>.Filter.And(
            Builders<Queue>.Filter.Eq(b => b.BrokerId, broker.Id),
            Builders<Queue>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Dto.QueueId))
        );

        var queueId = await queuesCollection.Find(queuesFilter).Project(x => x.Id).FirstAsync();

        var message = CreateMessageDtoMapper.ToMessage(queueId, user.Id, request.Dto);

        using var session = await mongoClient.StartSessionAsync();
        session.StartTransaction(new TransactionOptions(writeConcern: WriteConcern.WMajority));

        await messagesCollection.InsertOneAsync(message);
        await queuesCollection.UpdateOneAsync(x => x.Id == queueId,
            Builders<Queue>.Update.Inc(x => x.TotalMessages, 1));

        await session.CommitTransactionAsync();

        return OperationResult<CreateMessageFeatureResponse>.Success(new CreateMessageFeatureResponse());
    }
}