using System.Net.Http.Headers;
using System.Security.Claims;
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
    IHttpClientFactory httpClientFactory,
    UserManager<User> userManager,
    IMongoCollection<Queue> queuesCollection,
    IMongoCollection<Models.Broker> brokersCollection,
    IMongoCollection<Message> messagesCollection
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

        var http = httpClientFactory.CreateClient();
        http.BaseAddress = new Uri($"{broker.Url}:{broker.Port}");

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", broker.Auth);
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var requestBody = new
        {
            count = 10, // Number of messages to fetch
            ackmode = "ack_requeue_true", // Acknowledge and requeue the messages
            encoding = "auto",
            truncate = 50000
        };

        // vhost?
        var response =
            await http.PostAsync($"/api/queues/luqrhkdv/{queue.RawData.GetValue("name")}/get",
                new StringContent(JsonSerializer.Serialize(requestBody)));
        response.EnsureSuccessStatusCode();

        var content1 = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content1);
        var root = document.RootElement;

        var messages = new List<Message>();
        foreach (var element in root.EnumerateArray())
        {
            messages.Add(new Message()
            {
                QueueId = queue.Id,
                UserId = user.Id,
                RawData = BsonDocument.Parse(element.GetRawText()),
                Summary = "Contextual summary of messages",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (messages.Count > 0)
        {
            await messagesCollection.InsertManyAsync(messages);
        }

        return OperationResult<SyncMessagesFeatureResponse>.Success(new SyncMessagesFeatureResponse());
    }
}