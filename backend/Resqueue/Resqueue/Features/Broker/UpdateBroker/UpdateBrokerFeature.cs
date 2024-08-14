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
        if (!ObjectId.TryParse(request.Id, out var idObjectId))
        {
            return OperationResult<UpdateBrokerFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "Invalid broker ID"
            });
        }

        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<UpdateBrokerFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "Unauthorized access",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var filter = Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, idObjectId),
            Builders<Models.Broker>.Filter.Eq(b => b.UserId, user.Id)
        );

        var update = Builders<Models.Broker>.Update
            .Set(b => b.Auth, $"{request.Dto.Username}:{request.Dto.Password}")
            .Set(b => b.Port, request.Dto.Port)
            .Set(b => b.UpdatedAt, DateTime.UtcNow)
            .Set(b => b.Url, request.Dto.Url);

        await brokersCollection.UpdateOneAsync(filter, update);

        return OperationResult<UpdateBrokerFeatureResponse>.Success(new UpdateBrokerFeatureResponse());
    }
}