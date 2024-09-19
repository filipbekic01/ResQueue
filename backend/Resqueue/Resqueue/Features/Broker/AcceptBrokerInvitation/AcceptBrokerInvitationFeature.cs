using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Dtos.Broker;
using Resqueue.Enums;
using Resqueue.Models;

namespace Resqueue.Features.Broker.AcceptBrokerInvitation;

public record AcceptBrokerInvitationRequest(
    ClaimsPrincipal ClaimsPrincipal,
    AcceptBrokerInvitationDto Dto
);

public record AcceptBrokerInvitationResponse();

public class AcceptBrokerInvitationFeature(
    UserManager<User> userManager,
    IMongoClient mongoClient,
    IMongoCollection<Models.Broker> brokersCollection,
    IMongoCollection<BrokerInvitation> invitationsCollection
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

        // Get invitation
        var invitationFilter = Builders<BrokerInvitation>.Filter.And(
            Builders<BrokerInvitation>.Filter.Eq(b => b.Token, request.Dto.Token),
            Builders<BrokerInvitation>.Filter.Eq(b => b.InviteeId, userInvitee.Id)
        );
        var invitation = await invitationsCollection.Find(invitationFilter).FirstOrDefaultAsync();
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

        using var session = await mongoClient.StartSessionAsync();
        session.StartTransaction();

        // Get broker
        var brokerFilter = Builders<Models.Broker>.Filter.Eq(b => b.Id, invitation.BrokerId);
        var broker = await brokersCollection.Find(brokerFilter).FirstOrDefaultAsync();
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
        var updateBrokerInvitation = Builders<BrokerInvitation>.Update
            .Set(b => b.IsAccepted, true);

        await invitationsCollection.UpdateOneAsync(invitationFilter, updateBrokerInvitation);

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

        accessList.Add(new()
        {
            UserId = userInvitee.Id,
            AccessLevel = AccessLevel.Viewer
        });

        var updateBroker = Builders<Models.Broker>.Update
            .Set(b => b.AccessList, broker.AccessList);

        await brokersCollection.UpdateOneAsync(brokerFilter, updateBroker);

        await session.CommitTransactionAsync();

        return OperationResult<AcceptBrokerInvitationResponse>.Success(new());
    }
}