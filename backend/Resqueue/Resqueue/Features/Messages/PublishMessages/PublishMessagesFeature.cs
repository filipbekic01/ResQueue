using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;

namespace Resqueue.Features.Messages.PublishMessages;

public record PublishMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, PublishDto Dto);

public record PublishMessagesFeatureResponse();

public class PublishMessagesFeature(
    IHttpClientFactory httpClientFactory,
    IMongoCollection<Message> messagesCollection,
    IMongoCollection<Exchange> exchangesCollection,
    IMongoCollection<Models.Broker> brokersCollection,
    UserManager<User> userManager
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
        http.BaseAddress = new Uri($"{broker.Url}:{broker.Port}");

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", broker.Auth);
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Please note that the HTTP API is not ideal for high performance publishing; the need to create a new
        // TCP connection for each message published can limit message throughput compared to AMQP or other
        // protocols using long-lived connections.

        foreach (var message in messages)
        {
            var requestBody = new
            {
                properties = new { },
                routing_key = exchange.RawData.GetValue("name").ToString(),
                payload = message.RawData.GetValue("payload").ToString(),
                payload_encoding = message.RawData.GetValue("payload_encoding").ToString()
            };

            // vhost?
            var response =
                await http.PostAsync($"/api/exchanges/%2F/{exchange.RawData.GetValue("name")}/publish",
                    new StringContent(JsonSerializer.Serialize(requestBody)));

            response.EnsureSuccessStatusCode();
        }

        await messagesCollection.UpdateOneAsync(
            Builders<Message>.Filter
                .In(b => b.Id, messages.Select(x => x.Id).ToList()),
            Builders<Message>.Update
                .Set(b => b.DeletedAt, DateTime.UtcNow));

        return OperationResult<PublishMessagesFeatureResponse>.Success(new PublishMessagesFeatureResponse());
    }
}