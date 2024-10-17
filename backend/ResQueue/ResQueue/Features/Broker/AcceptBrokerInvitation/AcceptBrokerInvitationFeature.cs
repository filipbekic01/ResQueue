using System.Security.Claims;
using Marten;
using Marten.Patching;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResQueue.Constants;
using ResQueue.Dtos.Broker;
using ResQueue.Enums;
using ResQueue.Models;

namespace ResQueue.Features.Broker.AcceptBrokerInvitation;

public record AcceptBrokerInvitationRequest(
    ClaimsPrincipal ClaimsPrincipal,
    AcceptBrokerInvitationDto Dto
);

public record AcceptBrokerInvitationResponse();

public class AcceptBrokerInvitationFeature(
    UserManager<User> userManager,
    IDocumentSession documentSession
) : IAcceptBrokerInvitationFeature
{
    public async Task<OperationResult<AcceptBrokerInvitationResponse>> ExecuteAsync(
        AcceptBrokerInvitationRequest request)
    {
        // Get the current user
        var userInvitee = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (userInvitee == null)
        {
            return OperationResult<AcceptBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "The user is not authorized to accept this invitation.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        if (userInvitee.Subscription?.Type != StripePlans.ULTIMATE)
        {
            return OperationResult<AcceptBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "The user must be on Ultimate plan to accept invitation.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        // Get invitation

        var invitation = await documentSession.Query<BrokerInvitation>()
            .Where(x => x.Token == request.Dto.Token)
            .Where(x => x.InviteeId == userInvitee.Id)
            .FirstOrDefaultAsync();

        if (invitation == null)
        {
            return OperationResult<AcceptBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Invitation Not Found",
                Detail = "The invitation could not be found or does not exist.",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (DateTime.UtcNow > invitation.ExpiresAt)
        {
            return OperationResult<AcceptBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Invitation Expired",
                Detail = "The invitation has expired and is no longer valid.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (invitation.IsAccepted)
        {
            return OperationResult<AcceptBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Invitation Already Accepted",
                Detail = "This invitation has already been accepted.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Get broker
        var broker = await documentSession.Query<Models.Broker>()
            .Where(x => x.Id == invitation.BrokerId)
            .FirstOrDefaultAsync();
        if (broker == null)
        {
            return OperationResult<AcceptBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Broker Not Found",
                Detail = "The broker associated with this invitation could not be found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        // Update invitation
        documentSession.Patch<BrokerInvitation>(broker.Id)
            .Set(x => x.IsAccepted, true);

        // Update broker access list
        var accessList = broker.AccessList;
        if (accessList.Any(x => x.UserId == userInvitee.Id))
        {
            return OperationResult<AcceptBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Access Already Granted",
                Detail = "The user already has access to this broker.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        accessList.Add(new BrokerAccess
        {
            UserId = userInvitee.Id,
            AccessLevel = AccessLevel.Agent
        });

        documentSession.Patch<Models.Broker>(broker.Id)
            .Set(x => x.AccessList, broker.AccessList);

        await documentSession.SaveChangesAsync();

        return OperationResult<AcceptBrokerInvitationResponse>.Success(new());
    }
}