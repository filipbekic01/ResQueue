using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Dtos.Broker;
using Resqueue.Enums;
using Resqueue.Models;

namespace Resqueue.Features.Broker.ManageBrokerAccess;

public record ManageBrokerAccessFeatureRequest(
    ClaimsPrincipal ClaimsPrincipal,
    ManageBrokerAccessDto Dto,
    string BrokerId
);

public record ManageBrokerAccessFeatureResponse();

public class ManageBrokerAccessFeature(
    UserManager<User> userManager,
    IMongoCollection<Models.Broker> brokersCollection
) : IManageBrokerAccessFeature
{
    public async Task<OperationResult<ManageBrokerAccessFeatureResponse>> ExecuteAsync(
        ManageBrokerAccessFeatureRequest request)
    {
        // Get the current user
        var currentUser = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (currentUser == null)
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "You must be logged in to manage broker access.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        // Fetch the broker
        var brokerFilter = Builders<Models.Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(request.BrokerId));
        var broker = await brokersCollection.Find(brokerFilter).SingleAsync();

        // Check if current user is the owner
        var ownerAccess = broker.AccessList.FirstOrDefault(a =>
            a.UserId == currentUser.Id && a.AccessLevel == AccessLevel.Owner);

        if (ownerAccess == null)
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Forbidden",
                Detail =
                    "You do not have sufficient permissions to manage access. Only the broker owner can perform this action.",
                Status = StatusCodes.Status403Forbidden
            });
        }

        // Get the target user
        var targetUser = await userManager.FindByIdAsync(request.Dto.UserId);
        if (targetUser == null)
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "User Not Found",
                Detail = "The specified user could not be found in the system.",
                Status = StatusCodes.Status404NotFound
            });
        }

        // Find existing access
        var existingAccessIndex = broker.AccessList.FindIndex(a => a.UserId == targetUser.Id);
        if (existingAccessIndex >= 0)
        {
            if (request.Dto.AccessLevel != null)
            {
                // Update existing access level
                var update = Builders<Models.Broker>.Update
                    .Set(b => b.AccessList[existingAccessIndex].AccessLevel, request.Dto.AccessLevel.Value);

                var result = await brokersCollection.UpdateOneAsync(brokerFilter, update);

                if (result.ModifiedCount == 0)
                {
                    return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
                    {
                        Title = "Access Level Update Failed",
                        Detail = "Failed to update the user's access level. Please try again.",
                        Status = StatusCodes.Status500InternalServerError
                    });
                }
            }
            else
            {
                // Remove Access
                var update = Builders<Models.Broker>.Update.PullFilter(
                    b => b.AccessList,
                    a => a.UserId == targetUser.Id
                );

                var result = await brokersCollection.UpdateOneAsync(brokerFilter, update);

                if (result.ModifiedCount == 0)
                {
                    return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
                    {
                        Title = "Failed to Remove User",
                        Detail = "An error occurred while removing the user from the access list.",
                        Status = StatusCodes.Status500InternalServerError
                    });
                }
            }
        }
        else
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "User Does Not Have Access",
                Detail = "The specified user does not have access to this broker.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return OperationResult<ManageBrokerAccessFeatureResponse>.Success(new ManageBrokerAccessFeatureResponse());
    }
}