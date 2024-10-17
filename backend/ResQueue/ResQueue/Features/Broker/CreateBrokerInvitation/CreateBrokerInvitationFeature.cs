using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Marten;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using ResQueue.Constants;
using ResQueue.Dtos.Broker;
using ResQueue.Enums;
using ResQueue.Models;

namespace ResQueue.Features.Broker.CreateBrokerInvitation;

public record CreateBrokerInvitationRequest(
    ClaimsPrincipal ClaimsPrincipal,
    CreateBrokerInvitationDto Dto
);

public record CreateBrokerInvitationResponse();

public class CreateBrokerInvitationFeature(
    UserManager<User> userManager,
    IDocumentSession documentSession,
    IOptions<Settings> settings
) : ICreateBrokerInvitationFeature
{
    public async Task<OperationResult<CreateBrokerInvitationResponse>> ExecuteAsync(
        CreateBrokerInvitationRequest request)
    {
        // Get the user to invite
        var userInvitee = await documentSession.Query<User>()
            .Where(x => x.NormalizedEmail == request.Dto.Email.ToUpper())
            .FirstOrDefaultAsync();
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
        var hasInvitations = await documentSession.Query<BrokerInvitation>()
            .Where(x => x.IsAccepted == true)
            .Where(x => x.ExpiresAt > DateTime.UtcNow)
            .Where(x => x.InviteeId == userInvitee.Id)
            .AnyAsync();
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
        var broker = await documentSession.Query<Models.Broker>()
            .Where(x => x.Id == request.Dto.BrokerId)
            .FirstOrDefaultAsync();
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

        documentSession.Insert(invitation);

        await documentSession.SaveChangesAsync();

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
        message.From.Add(new MailboxAddress("ResQueue", "no-reply@resqueue.io"));
        message.To.Add(new MailboxAddress(currentUser.UserName, currentUser.Email));
        message.Subject = "ResQueue — Broker Invitation";

        var builder = new UriBuilder(settings.Value.WebsiteUrl)
        {
            Path = "app/broker-invitation",
            Query = $"token={token}"
        };

        message.Body = new TextPart("html")
        {
            Text = $"You're invited to join a team, <a href='{builder.Uri}'>click here</a> to proceed."
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(settings.Value.SmtpHost, settings.Value.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(settings.Value.SmtpUsername, settings.Value.SmtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}