using System.Security.Claims;
using Marten;
using Marten.Patching;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResQueue.Dtos.Broker;
using ResQueue.Enums;
using ResQueue.Models;

namespace ResQueue.Features.Broker.ManageBrokerAccess;

public record ManageBrokerAccessFeatureRequest(
    ClaimsPrincipal ClaimsPrincipal,
    ManageBrokerAccessDto Dto
);

public record ManageBrokerAccessFeatureResponse();

public class ManageBrokerAccessFeature(
    UserManager<User> userManager,
    IDocumentSession documentSession
) : IManageBrokerAccessFeature
{
    public async Task<OperationResult<ManageBrokerAccessFeatureResponse>> ExecuteAsync(
        ManageBrokerAccessFeatureRequest request)
    {
        // Get the current user
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "You must be logged in to manage broker access.",
                Status = StatusCodes.Status401Unauthorized
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

        // Fetch the broker
        var broker = await documentSession.Query<Models.Broker>()
            .Where(x => x.Id == request.Dto.BrokerId)
            .SingleAsync();

        if (request.Dto.AccessLevel is null)
        {
            return await RemoveAccess(request, broker, user, targetUser);
        }

        return await ChangeAccess(request, broker, user, targetUser);
    }

    private async Task<OperationResult<ManageBrokerAccessFeatureResponse>> RemoveAccess(
        ManageBrokerAccessFeatureRequest request,
        Models.Broker broker,
        User user,
        User targetUser)
    {
        // Check permissions
        if (user.Id != targetUser.Id &&
            !broker.AccessList.Any(x => x.UserId == user.Id && x.AccessLevel == AccessLevel.Owner))
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Forbidden",
                Detail = "You do not have sufficient permissions to manage access.",
                Status = StatusCodes.Status403Forbidden
            });
        }

        // There must be at least one owner left
        if (broker.AccessList.Any(x => x.AccessLevel == AccessLevel.Owner && x.UserId == targetUser.Id) &&
            broker.AccessList.Count(x => x.AccessLevel == AccessLevel.Owner) == 1)
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Forbidden",
                Detail = "As the sole owner, you cannot remove yourself from the broker.",
                Status = StatusCodes.Status403Forbidden
            });
        }

        // Remove access
        broker.AccessList = broker.AccessList.Where(x => x.UserId != targetUser.Id).ToList();

        documentSession.Patch<Models.Broker>(x => x.Id == broker.Id)
            .Set(x => x.AccessList, broker.AccessList);

        await documentSession.SaveChangesAsync();

        return OperationResult<ManageBrokerAccessFeatureResponse>.Success(new ManageBrokerAccessFeatureResponse());
    }

    private async Task<OperationResult<ManageBrokerAccessFeatureResponse>> ChangeAccess(
        ManageBrokerAccessFeatureRequest request,
        Models.Broker broker,
        User user,
        User targetUser)
    {
        // Check permissions

        if (user.Id == targetUser.Id)
        {
            return OperationResult<ManageBrokerAccessFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Forbidden",
                Detail = "You can't adjust your own permission settings.",
                Status = StatusCodes.Status403Forbidden
            });
        }

        var ownerAccess = broker.AccessList.FirstOrDefault(a =>
            a.UserId == user.Id && a.AccessLevel == AccessLevel.Owner);
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

        var access = broker.AccessList.Single(x => x.UserId == targetUser.Id);
        access.AccessLevel = request.Dto.AccessLevel!.Value;

        documentSession.Patch<Models.Broker>(x => x.Id == broker.Id)
            .Set(x => x.AccessList, broker.AccessList);

        await documentSession.SaveChangesAsync();

        return OperationResult<ManageBrokerAccessFeatureResponse>.Success(new ManageBrokerAccessFeatureResponse());
    }
}