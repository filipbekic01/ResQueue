using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQ.Client.Events;
using ResQueue.Mappers;
using ResQueue.Models;

namespace ResQueue.Features.Messages.SyncMessages;

public record SyncMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, string QueueId);

public record SyncMessagesFeatureResponse();

public class SyncMessagesFeature(
    UserManager<User> userManager,
    IMongoClient mongoClient,
    IMongoCollection<Queue> queuesCollection,
    IMongoCollection<Models.Broker> brokersCollection,
    IMongoCollection<Message> messagesCollection
) : ISyncMessagesFeature
{
    public async Task<OperationResult<SyncMessagesFeatureResponse>> ExecuteAsync(SyncMessagesFeatureRequest request)
    {
        // Get user
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<SyncMessagesFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "The user could not be found or is not logged in.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        // Get queue
        var queueFilter = Builders<Queue>.Filter.Eq(b => b.Id, ObjectId.Parse(request.QueueId));

        var queue = await queuesCollection.Find(queueFilter).SingleAsync();

        // Get broker
        var brokerFilter = Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, queue.BrokerId),
            Builders<Models.Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id)
        );
        var broker = await brokersCollection.Find(brokerFilter).SingleAsync();

        // RabbitMQ subscriber
        var factory = RabbitmqConnectionFactory.CreateAmqpFactory(broker);
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.BasicQos(0, 50, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        var queueName = queue.RawData.GetValue("name").AsString;

        // var tcs = new TaskCompletionSource();

        var conTag = Guid.NewGuid().ToString();

        consumer.Received += async (model, ea) =>
        {
            try
            {
                await SaveMessage(queueFilter, queue, user, ea);

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        var isRunning = true;
        consumer.ConsumerCancelled += (sender, args) =>
        {
            isRunning = false;
            return Task.CompletedTask;
        };
        consumer.Shutdown += (sender, args) => { return Task.CompletedTask; };

        channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumerTag: conTag,
            noLocal: false, // Must be false in RabbitMQ
            exclusive: false,
            new Dictionary<string, object>(),
            consumer: consumer
        );

        var isCancelled = false;
        while (!isCancelled)
        {
            var messageCount = channel.MessageCount(queueName);
            if (messageCount == 0)
            {
                channel.BasicCancel(conTag);
                isCancelled = true;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        while (isRunning)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        // Wait for the consumer to finish processing
        // await tcs.Task;

        return OperationResult<SyncMessagesFeatureResponse>.Success(new SyncMessagesFeatureResponse());
    }

    private async Task SaveMessage(FilterDefinition<Queue> queueFilter, Queue queue, User user,
        BasicDeliverEventArgs ea)
    {
        var queueWithNewSequence = await queuesCollection.FindOneAndUpdateAsync(queueFilter,
            Builders<Queue>.Update.Inc(x => x.NextMessageOrder, 1));

        var message = RabbitMQMessageMapper.ToDocument(
            queue.Id, user.Id, queueWithNewSequence.NextMessageOrder, ea);

        using var session = await mongoClient.StartSessionAsync();
        session.StartTransaction();

        // Insert the message into the messages collection
        await messagesCollection.InsertOneAsync(session, message);

        // Prepare the update pipeline
        var updatePipeline = new[]
        {
            // First stage: Update RawData.messages and Messages
            new BsonDocument("$set", new BsonDocument
            {
                {
                    "RawData.messages", new BsonDocument("$max", new BsonArray
                    {
                        0,
                        new BsonDocument("$subtract", new BsonArray { "$RawData.messages", 1 })
                    })
                },
                { "Messages", new BsonDocument("$add", new BsonArray { "$Messages", 1 }) }
            }),
            // Second stage: Update TotalMessages using updated values
            new BsonDocument("$set", new BsonDocument
            {
                {
                    "TotalMessages", new BsonDocument("$max", new BsonArray
                    {
                        0,
                        new BsonDocument("$add", new BsonArray
                        {
                            "$Messages",
                            "$RawData.messages"
                        })
                    })
                }
            })
        };

        // Apply the update to the queue
        await queuesCollection.UpdateOneAsync(session, x => x.Id == message.QueueId,
            Builders<Queue>.Update.Pipeline(updatePipeline));

        await session.CommitTransactionAsync();
    }
}