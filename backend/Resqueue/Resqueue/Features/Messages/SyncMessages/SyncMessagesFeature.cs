using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
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
            var props = new RabbitmqMessageProperties();

            if (res.BasicProperties.IsAppIdPresent())
            {
                // props.IsAppIdPresent = true;
                props.AppId = res.BasicProperties.AppId;
            }

            if (res.BasicProperties.IsClusterIdPresent())
            {
                // props.IsClusterIdPresent = true;
                props.ClusterId = res.BasicProperties.ClusterId;
            }

            if (res.BasicProperties.IsContentEncodingPresent())
            {
                // props.IsContentEncodingPresent = true;
                props.ContentEncoding = res.BasicProperties.ContentEncoding;
            }

            if (res.BasicProperties.IsContentTypePresent())
            {
                // props.IsContentTypePresent = true;
                props.ContentType = res.BasicProperties.ContentType;
            }

            if (res.BasicProperties.IsCorrelationIdPresent())
            {
                // props.IsCorrelationIdPresent = true;
                props.CorrelationId = res.BasicProperties.CorrelationId;
            }

            if (res.BasicProperties.IsDeliveryModePresent())
            {
                // props.IsDeliveryModePresent = true;
                props.DeliveryMode = res.BasicProperties.DeliveryMode;
            }

            if (res.BasicProperties.IsExpirationPresent())
            {
                // props.IsExpirationPresent = true;
                props.Expiration = res.BasicProperties.Expiration;
            }

            if (res.BasicProperties.IsHeadersPresent() && res.BasicProperties.Headers is not null)
            {
                // props.IsHeaderPresent = true;
                // props.Headers = res.BasicProperties.Headers;
                props.Headers = BsonDocument.Parse(JsonSerializer.Serialize(res.BasicProperties.Headers));
            }

            if (res.BasicProperties.IsMessageIdPresent())
            {
                // props.IsMessageIdPresent = true;
                props.MessageId = res.BasicProperties.MessageId;
            }

            if (res.BasicProperties.IsPriorityPresent())
            {
                // props.IsPriorityPresent = true;
                props.Priority = res.BasicProperties.Priority;
            }

            if (res.BasicProperties.IsReplyToPresent())
            {
                // props.IsReplyToPresent = true;
                props.ReplyTo = res.BasicProperties.ReplyTo;
            }

            if (res.BasicProperties.IsReplyToPresent())
            {
                // props.IsReplyToPresent = true;
                props.ReplyTo = res.BasicProperties.ReplyTo;
            }

            if (res.BasicProperties.IsTimestampPresent())
            {
                // props.IsTimestampPresent = true;
                props.Timestamp = res.BasicProperties.Timestamp.UnixTime;
            }

            if (res.BasicProperties.IsTypePresent())
            {
                // props.IsTypePresent = true;
                props.Type = res.BasicProperties.Type;
            }

            if (res.BasicProperties.IsUserIdPresent())
            {
                // props.IsUserIdPresent = true;
                props.UserId = res.BasicProperties.UserId;
            }

            await messagesCollection.InsertOneAsync(new Message
            {
                QueueId = queue.Id,
                UserId = user.Id,
                RabbitmqMetadata = new()
                {
                    Redelivered = res.Redelivered,
                    Exchange = res.Exchange,
                    RoutingKey = res.RoutingKey,
                    Properties = props
                },
                Body = BsonDocument.TryParse(Encoding.UTF8.GetString(res.Body.Span), out var doc)
                    ? doc
                    : new BsonBinaryData(res.Body.ToArray()),
                Summary = "Contextual summary of messages",
                CreatedAt = DateTime.UtcNow
            });

            channel.BasicAck(res.DeliveryTag, false);
        }

        return OperationResult<SyncMessagesFeatureResponse>.Success(new SyncMessagesFeatureResponse());
    }
}