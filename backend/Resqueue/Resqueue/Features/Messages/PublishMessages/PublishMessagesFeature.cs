using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;
using IBasicProperties = RabbitMQ.Client.IBasicProperties;

namespace Resqueue.Features.Messages.PublishMessages;

public record PublishMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, PublishDto Dto);

public record PublishMessagesFeatureResponse();

public class PublishMessagesFeature(
    IHttpClientFactory httpClientFactory,
    IMongoCollection<Message> messagesCollection,
    IMongoCollection<Exchange> exchangesCollection,
    IMongoCollection<Models.Broker> brokersCollection,
    UserManager<User> userManager,
    RabbitmqConnectionFactory rabbitmqConnectionFactory
) : IPublishMessagesFeature
{
    public async Task<OperationResult<PublishMessagesFeatureResponse>> ExecuteAsync(
        PublishMessagesFeatureRequest request)
    {
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<PublishMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "User not found"
            });
        }

        var exchange = await exchangesCollection
            .Find(Builders<Exchange>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Dto.ExchangeId)))
            .FirstOrDefaultAsync();

        if (exchange == null)
        {
            return OperationResult<PublishMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "Exchange not found"
            });
        }

        var broker = await brokersCollection.Find(Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, exchange.BrokerId),
            Builders<Models.Broker>.Filter.Eq(b => b.UserId, user.Id)
        )).FirstOrDefaultAsync();
        if (broker == null)
        {
            return OperationResult<PublishMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "Broker not found"
            });
        }

        var messagesFilter =
            Builders<Message>.Filter.In(b => b.Id, request.Dto.MessageIds.Select(ObjectId.Parse).ToList());
        var messages = await messagesCollection.Find(messagesFilter).ToListAsync();

        var http = httpClientFactory.CreateClient();
        http.BaseAddress = new Uri($"https://{broker.Host}:{broker.Port}");

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{broker.Username}:{broker.Password}")));

        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Please note that the HTTP API is not ideal for high performance publishing; the need to create a new
        // TCP connection for each message published can limit message throughput compared to AMQP or other
        // protocols using long-lived connections.

        var factory = rabbitmqConnectionFactory.CreateFactory(broker);
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        foreach (var message in messages)
        {
            var props = channel.CreateBasicProperties();

            if (message.RabbitMQMeta is not null)
            {
                if (message.RabbitMQMeta.Properties.AppId is not null)
                {
                    props.AppId = message.RabbitMQMeta.Properties.AppId;
                }

                if (message.RabbitMQMeta.Properties.ClusterId is not null)
                {
                    props.ClusterId = message.RabbitMQMeta.Properties.ClusterId;
                }

                if (message.RabbitMQMeta.Properties.ContentEncoding is not null)
                {
                    props.ContentEncoding = message.RabbitMQMeta.Properties.ContentEncoding;
                }

                if (message.RabbitMQMeta.Properties.ContentType is not null)
                {
                    props.ContentType = message.RabbitMQMeta.Properties.ContentType;
                }

                if (message.RabbitMQMeta.Properties.DeliveryMode is not null)
                {
                    props.DeliveryMode = message.RabbitMQMeta.Properties.DeliveryMode.Value;
                }

                if (message.RabbitMQMeta.Properties.Expiration is not null)
                {
                    props.Expiration = message.RabbitMQMeta.Properties.Expiration;
                }

                if (message.RabbitMQMeta.Properties.Headers is not null)
                {
                    props.Headers = message.RabbitMQMeta.Properties.Headers;
                }

                if (message.RabbitMQMeta.Properties.MessageId is not null)
                {
                    props.MessageId = message.RabbitMQMeta.Properties.MessageId;
                }

                if (message.RabbitMQMeta.Properties.Priority is not null)
                {
                    props.Priority = message.RabbitMQMeta.Properties.Priority.Value;
                }

                if (message.RabbitMQMeta.Properties.ReplyTo is not null)
                {
                    props.ReplyTo = message.RabbitMQMeta.Properties.ReplyTo;
                }

                if (message.RabbitMQMeta.Properties.Timestamp is not null)
                {
                    props.Timestamp = new(message.RabbitMQMeta.Properties.Timestamp.Value);
                }

                if (message.RabbitMQMeta.Properties.Type is not null)
                {
                    props.Type = message.RabbitMQMeta.Properties.Type;
                }

                if (message.RabbitMQMeta.Properties.UserId is not null)
                {
                    props.UserId = message.RabbitMQMeta.Properties.UserId;
                }
            }

            byte[] body = message.Body switch
            {
                BsonDocument doc => Encoding.UTF8.GetBytes(doc.ToJson()),
                BsonBinaryData bin => bin.Bytes,
                _ => throw new Exception($"Unsupported Body type {message.Body.GetType()}")
            };

            channel.BasicPublish(exchange.RawData.GetValue("name").AsString, "", false, props, body);

            await messagesCollection.UpdateOneAsync(
                Builders<Message>.Filter
                    .Eq(b => b.Id, message.Id),
                Builders<Message>.Update
                    .Set(b => b.DeletedAt, DateTime.UtcNow));
        }

        return OperationResult<PublishMessagesFeatureResponse>.Success(new PublishMessagesFeatureResponse());
    }
}