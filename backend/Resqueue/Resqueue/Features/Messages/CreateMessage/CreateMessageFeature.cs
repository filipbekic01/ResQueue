using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;

namespace Resqueue.Features.Messages.CreateMessage;

public record CreateMessageFeatureRequest(
    ClaimsPrincipal ClaimsPrincipal,
    CreateMessageDto Dto
);

public record CreateMessageFeatureResponse();

public class CreateMessageFeature(
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
        var queue = await queuesCollection.Find(queuesFilter).FirstAsync();

        // todo: move to mapper

        var message = new Message()
        {
            QueueId = queue.Id,
            UserId = user.Id,
            RabbitMQMeta = new()
            {
                Redelivered = false,
                Exchange = request.Dto.RabbitmqMetadata.Exchange,
                RoutingKey = request.Dto.RabbitmqMetadata.RoutingKey,
                Properties =
                    new RabbitMQMessageProperties
                    {
                        AppId = request.Dto.RabbitmqMetadata.Properties.AppId,
                        ClusterId = request.Dto.RabbitmqMetadata.Properties.ClusterId,
                        ContentEncoding = request.Dto.RabbitmqMetadata.Properties.ContentEncoding,
                        ContentType = request.Dto.RabbitmqMetadata.Properties.ContentType,
                        CorrelationId = request.Dto.RabbitmqMetadata.Properties.CorrelationId,
                        DeliveryMode = request.Dto.RabbitmqMetadata.Properties.DeliveryMode,
                        Expiration = request.Dto.RabbitmqMetadata.Properties.Expiration,
                        MessageId = request.Dto.RabbitmqMetadata.Properties.MessageId,
                        Priority = request.Dto.RabbitmqMetadata.Properties.Priority,
                        ReplyTo = request.Dto.RabbitmqMetadata.Properties.ReplyTo,
                        Timestamp = request.Dto.RabbitmqMetadata.Properties.Timestamp,
                        Type = request.Dto.RabbitmqMetadata.Properties.Type,
                        UserId = request.Dto.RabbitmqMetadata.Properties.UserId,
                    }
            },
            Body = BsonDocument.TryParse(request.Dto.Body.ToString(), out var doc)
                ? doc
                : new BsonBinaryData(request.Dto.Body.ToArray()),
            CreatedAt = DateTime.UtcNow
        };

        await messagesCollection.InsertOneAsync(message);

        // todo: increase counter

        return OperationResult<CreateMessageFeatureResponse>.Success(new CreateMessageFeatureResponse());
    }
}