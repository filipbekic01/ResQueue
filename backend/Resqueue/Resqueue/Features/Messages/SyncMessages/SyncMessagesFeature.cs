using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Mappers;
using Resqueue.Models;

namespace Resqueue.Features.Messages.SyncMessages;

public record SyncMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, string QueueId);

public record SyncMessagesFeatureResponse();

public class SyncMessagesFeature(
    UserManager<User> userManager,
    IMongoCollection<Queue> queuesCollection,
    IMongoCollection<Models.Broker> brokersCollection,
    IMongoCollection<Message> messagesCollection,
    RabbitmqConnectionFactory rabbitmqConnectionFactory
) : ISyncMessagesFeature
{
    public async Task<OperationResult<SyncMessagesFeatureResponse>> ExecuteAsync(SyncMessagesFeatureRequest request)
    {
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<SyncMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "User not found"
            });
        }

        var queueFilter = Builders<Queue>.Filter.Eq(b => b.Id, ObjectId.Parse(request.QueueId));
        var queue = await queuesCollection.Find(queueFilter).FirstOrDefaultAsync();
        if (queue == null)
        {
            return OperationResult<SyncMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "Queue not found"
            });
        }

        var brokerFilter = Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, queue.BrokerId),
            Builders<Models.Broker>.Filter.Eq(b => b.UserId, user.Id)
        );
        var broker = await brokersCollection.Find(brokerFilter).FirstOrDefaultAsync();
        if (broker == null)
        {
            return OperationResult<SyncMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "Broker not found"
            });
        }

        var factory = rabbitmqConnectionFactory.CreateFactory(broker);
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        while (channel.BasicGet(queue.RawData.GetValue("name").AsString, false) is { } res)
        {
            var message = RabbitMQMessageMapper.ToDocument(queue.Id, user.Id, res);
            await messagesCollection.InsertOneAsync(message);

            channel.BasicAck(res.DeliveryTag, false);
        }

        return OperationResult<SyncMessagesFeatureResponse>.Success(new SyncMessagesFeatureResponse());
    }
}