using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Dtos;
using ResQueue.Enums;
using ResQueue.Models;

namespace ResQueue.Features.Broker.UpdateBroker;

public record UpdateBrokerFeatureRequest(ClaimsPrincipal ClaimsPrincipal, UpdateBrokerDto Dto, string Id);

public record UpdateBrokerFeatureResponse();

public class UpdateBrokerFeature(
    UserManager<User> userManager,
    IMongoCollection<Models.Broker> brokersCollection
) : IUpdateBrokerFeature
{
    public async Task<OperationResult<UpdateBrokerFeatureResponse>> ExecuteAsync(UpdateBrokerFeatureRequest request)
    {
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<UpdateBrokerFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "You must be logged in to sync the broker data.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var filter = Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Id)),
            Builders<Models.Broker>.Filter.ElemMatch(b => b.AccessList,
                a => a.UserId == user.Id &&
                     (a.AccessLevel == AccessLevel.Owner || a.AccessLevel == AccessLevel.Manager))
        );

        var update = Builders<Models.Broker>.Update
            .Set(b => b.Name, request.Dto.Name)
            .Set(b => b.Settings, new BrokerSettings()
            {
                QuickSearches = request.Dto.Settings.QuickSearches,
                DeadLetterQueueSuffix = request.Dto.Settings.DeadLetterQueueSuffix,
                MessageFormat = request.Dto.Settings.MessageFormat,
                MessageStructure = request.Dto.Settings.MessageStructure,
                QueueTrimPrefix = request.Dto.Settings.QueueTrimPrefix,
                DefaultQueueSortField = request.Dto.Settings.DefaultQueueSortField,
                DefaultQueueSortOrder = request.Dto.Settings.DefaultQueueSortOrder,
                DefaultQueueSearch = request.Dto.Settings.DefaultQueueSearch
            })
            .Set(b => b.UpdatedAt, DateTime.UtcNow);

        if (request.Dto.RabbitMqConnection is { } rabbitMqConnection)
        {
            if (!string.IsNullOrEmpty(rabbitMqConnection.Username) &&
                !string.IsNullOrEmpty(rabbitMqConnection.Password))
            {
                update = update
                    .Set(b => b.RabbitMQConnection!.Username, rabbitMqConnection.Username)
                    .Set(b => b.RabbitMQConnection!.Password, rabbitMqConnection.Password);
            }

            update = update
                .Set(b => b.RabbitMQConnection!.ManagementPort, rabbitMqConnection.ManagementPort)
                .Set(b => b.RabbitMQConnection!.ManagementTls, rabbitMqConnection.ManagementTls)
                .Set(b => b.RabbitMQConnection!.AmqpPort, rabbitMqConnection.AmqpPort)
                .Set(b => b.RabbitMQConnection!.AmqpTls, rabbitMqConnection.AmqpTls)
                .Set(b => b.RabbitMQConnection!.Host, rabbitMqConnection.Host)
                .Set(b => b.RabbitMQConnection!.VHost, rabbitMqConnection.VHost);
        }

        await brokersCollection.UpdateOneAsync(filter, update);

        return OperationResult<UpdateBrokerFeatureResponse>.Success(new UpdateBrokerFeatureResponse());
    }
}