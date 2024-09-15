using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
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
                Detail = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        // Validate Broker ID
        if (!ObjectId.TryParse(request.BrokerId, out var brokerId))
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Detail = "Invalid Broker ID",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Fetch the broker
        var brokerFilter = Builders<Models.Broker>.Filter.Eq(b => b.Id, brokerId);
        var broker = await brokersCollection.Find(brokerFilter).FirstOrDefaultAsync();

        if (broker == null)
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Detail = "Broker not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        // Check if current user is the owner
        var ownerAccess = broker.AccessList.FirstOrDefault(a =>
            a.UserId == currentUser.Id && a.AccessLevel == AccessLevel.Owner);

        if (ownerAccess == null)
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Detail = "Forbidden: Only the owner can manage access.",
                Status = StatusCodes.Status403Forbidden
            });
        }

        // Prevent owner from removing themselves
        if (request.Dto.AccessLevel == null && request.Dto.UserId == currentUser.Id.ToString())
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Detail = "Cannot remove the owner from the access list.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Get the target user
        var targetUser = await userManager.FindByIdAsync(request.Dto.UserId);
        if (targetUser == null)
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Detail = "User not found",
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
                        Detail = "Failed to update user access level",
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
                        Detail = "Failed to remove user from access list",
                        Status = StatusCodes.Status500InternalServerError
                    });
                }
            }
        }
        else
        {
            // User not found in access list
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Detail = "User does not have access to this broker",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return OperationResult<ManageBrokerAccessFeatureResponse>.Success(new ManageBrokerAccessFeatureResponse());
    }
}