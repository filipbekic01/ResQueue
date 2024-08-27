using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;

namespace Resqueue.Features.Broker.UpdateBroker;

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
            return OperationResult<UpdateBrokerFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var filter = Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Id)),
            Builders<Models.Broker>.Filter.Eq(b => b.UserId, user.Id)
        );

        var update = Builders<Models.Broker>.Update
            .Set(b => b.Port, request.Dto.Port)
            .Set(b => b.Host, request.Dto.Host)
            .Set(b => b.Name, request.Dto.Name)
            .Set(b => b.Settings, new BrokerSettings()
            {
                QuickSearches = request.Dto.Settings.QuickSearches
            })
            .Set(b => b.UpdatedAt, DateTime.UtcNow);

        if (!string.IsNullOrEmpty(request.Dto.Username) && !string.IsNullOrEmpty(request.Dto.Password))
        {
            update = update
                .Set(b => b.Username, request.Dto.Username)
                .Set(b => b.Password, request.Dto.Password);
        }

        await brokersCollection.UpdateOneAsync(filter, update);

        return OperationResult<UpdateBrokerFeatureResponse>.Success(new UpdateBrokerFeatureResponse());
    }
}