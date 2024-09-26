using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Dtos;
using ResQueue.Models;

namespace ResQueue.Features.Messages.PublishMessages;

public record PublishMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, PublishDto Dto);

public record PublishMessagesFeatureResponse();

public class PublishMessagesFeature(
    IMongoCollection<Message> messagesCollection,
    IMongoCollection<Exchange> exchangesCollection,
    IMongoCollection<Queue> queuesCollection,
    IMongoCollection<Models.Broker> brokersCollection,
    UserManager<User> userManager,
    IMongoClient mongoClient
) : IPublishMessagesFeature
{
    public async Task<OperationResult<PublishMessagesFeatureResponse>> ExecuteAsync(
        PublishMessagesFeatureRequest request)
    {
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<PublishMessagesFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "The user could not be found or is not logged in.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var broker = await brokersCollection
            .Find(Builders<Models.Broker>.Filter.And(
                Builders<Models.Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Dto.BrokerId)),
                Builders<Models.Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id)
            ))
            .SingleAsync();

        var exchange = await exchangesCollection
            .Find(Builders<Exchange>.Filter.And(
                Builders<Exchange>.Filter.Eq(b => b.BrokerId, broker.Id),
                Builders<Exchange>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Dto.ExchangeId))
            ))
            .SingleAsync();

        var queue = await queuesCollection
            .Find(Builders<Queue>.Filter.And(
                Builders<Queue>.Filter.Eq(b => b.BrokerId, broker.Id),
                Builders<Queue>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Dto.QueueId))
            ))
            .SingleAsync();

        var messagesFilter =
            Builders<Message>.Filter.In(b => b.Id, request.Dto.MessageIds.Select(ObjectId.Parse).ToList());

        var sort = Builders<Message>.Sort.Ascending(q => q.MessageOrder);

        var factory = RabbitmqConnectionFactory.CreateAmqpFactory(broker);
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        await messagesCollection
            .Find(messagesFilter)
            .Sort(sort)
            .ForEachAsync(async message =>
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
                    BsonString str => Encoding.UTF8.GetBytes(str.Value),
                    BsonBinaryData bin => bin.Bytes,
                    _ => throw new Exception($"Unsupported Body type {message.Body.GetType()}")
                };

                channel.BasicPublish(exchange.RawData.GetValue("name").AsString, "", false, props, body);

                using var session = await mongoClient.StartSessionAsync();
                session.StartTransaction();

                // Define the update pipeline
                var updatePipeline = new[]
                {
                    // First stage: Decrement Messages and ensure it's not less than 0
                    new BsonDocument("$set", new BsonDocument
                    {
                        {
                            "Messages", new BsonDocument("$max", new BsonArray
                            {
                                0,
                                new BsonDocument("$subtract", new BsonArray { "$Messages", 1 })
                            })
                        }
                    }),
                    // Second stage: Update TotalMessages using the updated Messages and RawData.messages
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
                    x => x.Id == queue.Id,
                    Builders<Queue>.Update.Pipeline(updatePipeline)
                );

                await messagesCollection.UpdateOneAsync(
                    session,
                    Builders<Message>.Filter.Eq(b => b.Id, message.Id),
                    Builders<Message>.Update.Set(b => b.DeletedAt, DateTime.UtcNow)
                );

                await session.CommitTransactionAsync();
            });

        return OperationResult<PublishMessagesFeatureResponse>.Success(new PublishMessagesFeatureResponse());
    }
}