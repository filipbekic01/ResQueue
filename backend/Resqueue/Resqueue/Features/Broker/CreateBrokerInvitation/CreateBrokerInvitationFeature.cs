using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using MongoDB.Bson;
using MongoDB.Driver;
using Org.BouncyCastle.Asn1.X509;
using Resqueue.Constants;
using Resqueue.Dtos;
using Resqueue.Dtos.Broker;
using Resqueue.Enums;
using Resqueue.Models;

namespace Resqueue.Features.Broker.CreateBrokerInvitation;

public record CreateBrokerInvitationRequest(
    ClaimsPrincipal ClaimsPrincipal,
    CreateBrokerInvitationDto Dto
);

public record CreateBrokerInvitationResponse();

public class CreateBrokerInvitationFeature(
    UserManager<User> userManager,
    IMongoCollection<Models.Broker> brokersCollection,
    IMongoCollection<User> usersCollection,
    IMongoCollection<BrokerInvitation> invitationsCollection,
    IOptions<Settings> settings
) : ICreateBrokerInvitationFeature
{
    public async Task<OperationResult<CreateBrokerInvitationResponse>> ExecuteAsync(
        CreateBrokerInvitationRequest request)
    {
        // Get the user to invite
        var inviteeFilter = Builders<User>.Filter.Eq(b => b.NormalizedEmail, request.Dto.Email.ToUpper());
        var userInvitee = await usersCollection.Find(inviteeFilter).FirstOrDefaultAsync();
        if (userInvitee == null)
        {
            return OperationResult<CreateBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "User Not Found",
                Detail = "The invitee could not be found in the system.",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (userInvitee.Subscription?.Type != StripePlans.ULTIMATE)
        {
            return OperationResult<CreateBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Invalid Plan",
                Detail = "The invitee must have the Ultimate plan to receive an invitation.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Get the current user
        var userInviter = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (userInviter is null)
        {
            return OperationResult<CreateBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "You are not authorized to send invitations.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        if (userInviter.Subscription?.Type != StripePlans.ULTIMATE)
        {
            return OperationResult<CreateBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Invalid Plan",
                Detail = "You must have the Ultimate plan to send an invitation.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Get the current user
        var brokerInvitationFilter = Builders<BrokerInvitation>.Filter.And(
            Builders<BrokerInvitation>.Filter.Eq(b => b.IsAccepted, false),
            Builders<BrokerInvitation>.Filter.Gt(b => b.ExpiresAt, DateTime.UtcNow),
            Builders<BrokerInvitation>.Filter.Eq(b => b.InviteeId, userInvitee.Id)
        );
        var hasInvitations = await invitationsCollection.Find(brokerInvitationFilter).AnyAsync();
        if (hasInvitations)
        {
            return OperationResult<CreateBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Duplicate Invitation",
                Detail = "The user has already been invited and has a pending invitation.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Fetch the broker
        var brokerFilter = Builders<Models.Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(request.Dto.BrokerId));
        var broker = await brokersCollection.Find(brokerFilter).FirstOrDefaultAsync();
        if (broker == null)
        {
            return OperationResult<CreateBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Broker Not Found",
                Detail = "The specified broker could not be found in the system.",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (!broker.AccessList.Any(x => x.UserId == userInviter.Id && x.AccessLevel == AccessLevel.Owner))
        {
            return OperationResult<CreateBrokerInvitationResponse>.Failure(new ProblemDetails
            {
                Title = "Forbidden",
                Detail = "Only owners are allowed to create invitations.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var invitation = new BrokerInvitation()
        {
            BrokerId = broker.Id,
            InviterId = userInviter.Id,
            InviterEmail = userInviter.Email!,
            InviteeId = userInvitee.Id,
            Token = GenerateRandomString(32),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            BrokerName = broker.Name,
        };

        await invitationsCollection.InsertOneAsync(invitation);

        await SendEmail(userInvitee, invitation.Token);

        return OperationResult<CreateBrokerInvitationResponse>.Success(new());
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var randomBytes = new byte[length];
        var result = new StringBuilder(length);

        using (var range = RandomNumberGenerator.Create())
        {
            range.GetBytes(randomBytes);
        }

        foreach (var randomByte in randomBytes)
        {
            result.Append(chars[randomByte % chars.Length]);
        }

        return result.ToString();
    }

    private async Task SendEmail(User currentUser, string token)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("ResQueue", "resqueue@no-reply.io"));
        message.To.Add(new MailboxAddress(currentUser.UserName, currentUser.Email));
        message.Subject = "ResQueue — Broker Invitation";

        message.Body = new TextPart("html")
        {
            Text = $"Broker invitation: {token}"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(settings.Value.SmtpHost, settings.Value.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(settings.Value.SmtpUsername, settings.Value.SmtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}