using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Models;

namespace Resqueue.Features.Broker.SyncBroker;

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
    IMongoCollection<Exchange> exchangesCollection
) : ISyncBrokerFeature
{
    public async Task<OperationResult<SyncBrokerFeatureResponse>> ExecuteAsync(SyncBrokerFeatureRequest request)
    {
        var dt = DateTime.UtcNow;

        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<SyncBrokerFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "Unauthorized access",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var brokerFilter = Builders<Models.Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Id));
        var broker = await brokersCollection.Find(brokerFilter).FirstOrDefaultAsync();
        if (broker == null)
        {
            return OperationResult<SyncBrokerFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = $"Broker {request.Id} not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var queuesFilter = Builders<Queue>.Filter.Eq(b => b.BrokerId, ObjectId.Parse(request.Id));
        var queues = await queuesCollection.Find(queuesFilter).ToListAsync();

        var exchangesFilters = Builders<Exchange>.Filter.Eq(b => b.BrokerId, ObjectId.Parse(request.Id));
        var exchanges = await exchangesCollection.Find(exchangesFilters).ToListAsync();

        var http = httpClientFactory.CreateClient();

        http.BaseAddress = new Uri($"{broker.Url}:{broker.Port}");

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", broker.Auth);
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Sync queues

        var response = await http.GetAsync($"/api/queues");
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

            if (!queues.Any(queue =>
                    queue.RawData.TryGetValue("name", out var nameValue) && nameValue == queueName))
            {
                queuesToAdd.Add(new Queue
                {
                    BrokerId = ObjectId.Parse(request.Id),
                    UserId = user.Id,
                    RawData = BsonDocument.Parse(element.GetRawText()),
                    CreatedAt = dt
                });
            }
            else
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
                    queuesToUpdate.Add(queue);
                }
            }
        }

        foreach (var queue in queues)
        {
            if (!queue.RawData.TryGetValue("name", out var nameValue))
            {
                continue;
            }

            var queueName = nameValue.ToString();

            if (!root.EnumerateArray().Any(element =>
                    element.TryGetProperty("name", out var nameProperty) &&
                    nameProperty.ToString() == queueName))
            {
                queueIdsToDelete.Add(queue.Id);
            }
        }

        if (queueIdsToDelete.Count > 0)
        {
            var deleteFilter = Builders<Queue>.Filter.In(q => q.Id, queueIdsToDelete);
            await queuesCollection.DeleteManyAsync(deleteFilter);
        }

        if (queuesToAdd.Count > 0)
        {
            await queuesCollection.InsertManyAsync(queuesToAdd);
        }

        if (queuesToUpdate.Count > 0)
        {
            var bulkOperations = new List<WriteModel<Queue>>();

            foreach (var queue in queuesToUpdate)
            {
                var updateFilter = Builders<Queue>.Filter.Eq(q => q.Id, queue.Id);
                var updateDefinition = Builders<Queue>.Update.Set(q => q.RawData, queue.RawData);

                var updateOneModel = new UpdateOneModel<Queue>(updateFilter, updateDefinition);
                bulkOperations.Add(updateOneModel);
            }

            if (bulkOperations.Count > 0)
            {
                await queuesCollection.BulkWriteAsync(bulkOperations);
            }
        }

        // Sync exchanges

        response = await http.GetAsync($"/api/exchanges");
        response.EnsureSuccessStatusCode();

        var content2 = await response.Content.ReadAsStringAsync();
        using var document2 = JsonDocument.Parse(content2);
        var root2 = document2.RootElement;

        var exchangesToAdd = new List<Exchange>();
        var exchangeIdsToDelete = new List<ObjectId>();

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
                    UserId = user.Id,
                    BrokerId = ObjectId.Parse(request.Id),
                    RawData = BsonDocument.Parse(element.GetRawText())
                });
            }
        }

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

        if (exchangeIdsToDelete.Count > 0)
        {
            var deleteFilter = Builders<Exchange>.Filter.In(q => q.Id, exchangeIdsToDelete);
            await exchangesCollection.DeleteManyAsync(deleteFilter);
        }

        if (exchangesToAdd.Count > 0)
        {
            await exchangesCollection.InsertManyAsync(exchangesToAdd);
        }

        var update = Builders<Models.Broker>.Update
            .Set(b => b.SyncedAt, DateTime.UtcNow);

        var filter = Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, broker.Id)
        );

        await brokersCollection.UpdateOneAsync(filter, update);

        return OperationResult<SyncBrokerFeatureResponse>.Success(new SyncBrokerFeatureResponse());
    }
}