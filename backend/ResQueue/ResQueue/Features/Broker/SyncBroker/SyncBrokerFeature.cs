using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Models;

namespace ResQueue.Features.Broker.SyncBroker;

public record SyncBrokerFeatureRequest(
    ClaimsPrincipal ClaimsPrincipal,
    string Id
);

public record SyncBrokerFeatureResponse();

public class SyncBrokerFeature(
    IHttpClientFactory httpClientFactory,
    UserManager<User> userManager,
    IMongoCollection<Queue> queuesCollection,
    IMongoCollection<Models.Broker> brokersCollection,
    IMongoCollection<Exchange> exchangesCollection,
    IMongoClient mongoClient
) : ISyncBrokerFeature
{
    public async Task<OperationResult<SyncBrokerFeatureResponse>> ExecuteAsync(SyncBrokerFeatureRequest request)
    {
        var dt = DateTime.UtcNow;

        // Get user
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<SyncBrokerFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "You must be logged in to sync the broker data.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        // Get broker
        var brokerFilter = Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id),
            Builders<Models.Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Id))
        );
        var broker = await brokersCollection.Find(brokerFilter).SingleAsync();

        // Get queues
        var queuesFilter = Builders<Queue>.Filter.Eq(b => b.BrokerId, ObjectId.Parse(request.Id));
        var queues = await queuesCollection.Find(queuesFilter).ToListAsync();

        // Get exchanges
        var exchangesFilters = Builders<Exchange>.Filter.Eq(b => b.BrokerId, ObjectId.Parse(request.Id));
        var exchanges = await exchangesCollection.Find(exchangesFilters).ToListAsync();

        using var http = RabbitmqConnectionFactory.CreateManagementClient(httpClientFactory, broker);

        // Sync queues
        var response = await http.GetAsync("/api/queues");
        response.EnsureSuccessStatusCode();

        var content1 = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content1);
        var root = document.RootElement;

        var queuesToAdd = new List<Queue>();
        var queuesToUpdate = new List<Queue>();
        var queueIdsToDelete = new List<ObjectId>();

        foreach (var element in root.EnumerateArray())
        {
            element.TryGetProperty("name", out var nameProperty);
            var queueName = nameProperty.ToString();

            var queueExists = queues.Any(queue =>
                queue.RawData.TryGetValue("name", out var nameValue) && nameValue == queueName);

            // Add queue
            if (!queueExists)
            {
                queuesToAdd.Add(new Queue
                {
                    BrokerId = ObjectId.Parse(request.Id),
                    RawData = BsonDocument.Parse(element.GetRawText()),
                    CreatedAt = dt
                });
            }

            // Update queue
            if (queueExists)
            {
                var queue = queues.Find(x =>
                    x.RawData.TryGetValue("name", out var nameValue) && nameValue == queueName);

                if (queue is null)
                {
                    continue;
                }

                var newRawData = BsonDocument.Parse(element.GetRawText());

                if (!queue.RawData.Equals(newRawData))
                {
                    queue.RawData = newRawData;
                    queue.TotalMessages = queue.Messages + queue.RawData["messages"].AsInt32;
                    queuesToUpdate.Add(queue);
                }
            }
        }

        // Delete queue
        foreach (var queue in queues)
        {
            // Skip if local messages exist
            if (queue.Messages > 0)
            {
                continue;
            }

            if (!queue.RawData.TryGetValue("name", out var nameValue))
            {
                continue;
            }

            if (root.EnumerateArray().Any(element =>
                    element.TryGetProperty("name", out var nameProperty) &&
                    nameProperty.ToString() == nameValue.ToString()))
            {
                continue;
            }

            queueIdsToDelete.Add(queue.Id);
        }

        // Sync exchanges
        response = await http.GetAsync($"/api/exchanges");
        response.EnsureSuccessStatusCode();

        var content2 = await response.Content.ReadAsStringAsync();
        using var document2 = JsonDocument.Parse(content2);
        var root2 = document2.RootElement;

        var exchangesToAdd = new List<Exchange>();
        var exchangeIdsToDelete = new List<ObjectId>();

        // Add exchange
        foreach (var element in root2.EnumerateArray())
        {
            if (!element.TryGetProperty("name", out var nameProperty))
            {
                continue;
            }

            var exchangeName = nameProperty.ToString();

            if (!exchanges.Any(exchange =>
                    exchange.RawData.TryGetValue("name", out var nameValue) && nameValue == exchangeName))
            {
                exchangesToAdd.Add(new Exchange
                {
                    BrokerId = ObjectId.Parse(request.Id),
                    RawData = BsonDocument.Parse(element.GetRawText())
                });
            }
        }

        // Remove exchange
        foreach (var exchange in exchanges)
        {
            if (!exchange.RawData.TryGetValue("name", out var nameValue))
            {
                continue;
            }

            var exchangeName = nameValue.ToString();

            if (!root2.EnumerateArray().Any(element =>
                    element.TryGetProperty("name", out var nameProperty) &&
                    nameProperty.ToString() == exchangeName))
            {
                exchangeIdsToDelete.Add(exchange.Id);
            }
        }

        // Do transaction 
        using var session = await mongoClient.StartSessionAsync();
        session.StartTransaction();

        // queues
        if (queueIdsToDelete.Count > 0)
        {
            var deleteFilter = Builders<Queue>.Filter.In(q => q.Id, queueIdsToDelete);
            await queuesCollection.DeleteManyAsync(session, deleteFilter);
        }

        if (queuesToAdd.Count > 0)
        {
            await queuesCollection.InsertManyAsync(session, queuesToAdd);
        }

        if (queuesToUpdate.Count > 0)
        {
            var bulkOperations = new List<WriteModel<Queue>>();

            foreach (var queue in queuesToUpdate)
            {
                var updateFilter = Builders<Queue>.Filter.Eq(q => q.Id, queue.Id);
                var updateDefinition = Builders<Queue>.Update
                    .Set(q => q.RawData, queue.RawData)
                    .Set(q => q.TotalMessages, queue.TotalMessages);

                var updateOneModel = new UpdateOneModel<Queue>(updateFilter, updateDefinition);
                bulkOperations.Add(updateOneModel);
            }

            await queuesCollection.BulkWriteAsync(session, bulkOperations);
        }

        // Exchanges
        if (exchangeIdsToDelete.Count > 0)
        {
            var deleteFilter = Builders<Exchange>.Filter.In(q => q.Id, exchangeIdsToDelete);
            await exchangesCollection.DeleteManyAsync(session, deleteFilter);
        }

        if (exchangesToAdd.Count > 0)
        {
            await exchangesCollection.InsertManyAsync(session, exchangesToAdd);
        }

        // Broker
        var update = Builders<Models.Broker>.Update
            .Set(b => b.SyncedAt, DateTime.UtcNow);

        var filter = Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, broker.Id)
        );

        await brokersCollection.UpdateOneAsync(session, filter, update);

        await session.CommitTransactionAsync();

        return OperationResult<SyncBrokerFeatureResponse>.Success(new SyncBrokerFeatureResponse());
    }
}