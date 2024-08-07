using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Constants;
using Resqueue.Dtos;
using Resqueue.Models;
using Resqueue.Models.MongoDB;

namespace Resqueue.Endpoints;

public static class BrokerEndpoints
{
    public static IEndpointRouteBuilder MapBrokerEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("brokers")
            .RequireAuthorization();

        group.MapGet("",
            async (IMongoCollection<Broker> collection, UserManager<User> userManager, HttpContext httpContext) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Broker>.Filter.Eq(b => b.UserId, user.Id);
                var brokers = await collection.Find(filter).ToListAsync();
                var dtos = brokers.Select(x => new BrokerDto(
                    x.Id.ToString(),
                    x.Name,
                    x.Port,
                    x.Url,
                    x.Framework,
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.SyncedAt,
                    x.DeletedAt
                ));

                return Results.Ok(dtos);
            });

        group.MapPost("",
            async (IMongoCollection<Broker> collection, [FromBody] CreateBrokerDto dto, UserManager<User> userManager,
                HttpContext httpContext) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                if (!string.IsNullOrEmpty(dto.Framework))
                {
                    if (dto.Framework.ToLower() != Frameworks.MASS_TRANSIT)
                    {
                        return Results.BadRequest("Invalid framework.");
                    }
                }

                byte[] authBytes = Encoding.UTF8.GetBytes($"{dto.Username}:{dto.Password}");
                string authBase64 = Convert.ToBase64String(authBytes);

                var dateTime = DateTime.UtcNow;

                var broker = new Broker
                {
                    Name = dto.Name,
                    Auth = authBase64,
                    Port = dto.Port,
                    Url = dto.Url,
                    Framework = string.IsNullOrEmpty(dto.Framework) ? Frameworks.NONE : dto.Framework.ToLower(),
                    UserId = user.Id,
                    CreatedAt = dateTime,
                    UpdatedAt = dateTime
                };

                await collection.InsertOneAsync(broker);

                return Results.Ok();
            });

        group.MapPost("{brokerId}/sync",
            async (IMongoCollection<Queue> queuesCollection, IMongoCollection<Broker> brokersCollection,
                IMongoCollection<Exchange> exchangesCollection,
                [FromBody] CreateBrokerDto dto, UserManager<User> userManager,
                HttpContext httpContext, IHttpClientFactory httpClientFactory, string brokerId) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var brokerFilter = Builders<Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(brokerId));
                var broker = await brokersCollection.Find(brokerFilter).FirstOrDefaultAsync();
                if (broker == null)
                {
                    return Results.Unauthorized();
                }

                var queuesFilter = Builders<Queue>.Filter.Eq(b => b.BrokerId, ObjectId.Parse(brokerId));
                var queues = await queuesCollection.Find(queuesFilter).ToListAsync();

                var exchangesFilters = Builders<Exchange>.Filter.Eq(b => b.BrokerId, ObjectId.Parse(brokerId));
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
                var queueIdsToDelete = new List<ObjectId>();

                foreach (var element in root.EnumerateArray())
                {
                    if (!element.TryGetProperty("name", out var nameProperty))
                    {
                        continue;
                    }

                    var queueName = nameProperty.ToString();

                    if (!queues.Any(queue =>
                            queue.RawData.TryGetValue("name", out var nameValue) && nameValue == queueName))
                    {
                        queuesToAdd.Add(new Queue
                        {
                            BrokerId = new ObjectId(brokerId),
                            UserId = user.Id,
                            RawData = BsonDocument.Parse(element.GetRawText())
                        });
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
                            BrokerId = new ObjectId(brokerId),
                            UserId = user.Id,
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

                var update = Builders<Broker>.Update
                    .Set(b => b.SyncedAt, DateTime.UtcNow);

                var filter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, broker.Id)
                );

                await brokersCollection.UpdateOneAsync(filter, update);

                return Results.Ok();
            });

        group.MapPut("/{id}",
            async (string id, [FromBody] UpdateBrokerDto updateBrokerDto, UserManager<User> userManager,
                HttpContext httpContext, IMongoCollection<Broker> collection) =>
            {
                if (!ObjectId.TryParse(id, out var objectId))
                {
                    return Results.BadRequest("Invalid ID format.");
                }

                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, objectId),
                    Builders<Broker>.Filter.Eq(b => b.UserId, user.Id)
                );

                var update = Builders<Broker>.Update
                    .Set(b => b.Auth, $"{updateBrokerDto.Username}:{updateBrokerDto.Password}")
                    .Set(b => b.Port, updateBrokerDto.Port)
                    .Set(b => b.UpdatedAt, DateTime.UtcNow)
                    .Set(b => b.Url, updateBrokerDto.Url);

                var result = await collection.UpdateOneAsync(filter, update);

                return result.MatchedCount == 0 ? Results.NotFound() : Results.NoContent();
            });

        group.MapDelete("{id}",
            async (IMongoCollection<Broker> collection, UserManager<User> userManager, HttpContext httpContext,
                string id) =>
            {
                if (!ObjectId.TryParse(id, out var objectId))
                {
                    return Results.BadRequest("Invalid ID format.");
                }

                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, objectId),
                    Builders<Broker>.Filter.Eq(b => b.UserId, user.Id)
                );

                var result = await collection.DeleteOneAsync(filter);

                return result.DeletedCount == 0 ? Results.NotFound() : Results.Ok();
            });

        return group;
    }
}