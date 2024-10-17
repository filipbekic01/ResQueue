using System.Security.Claims;
using Marten;
using Marten.Patching;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResQueue.Dtos;
using ResQueue.Enums;
using ResQueue.Models;

namespace ResQueue.Features.Broker.UpdateBroker;

public record UpdateBrokerFeatureRequest(ClaimsPrincipal ClaimsPrincipal, UpdateBrokerDto Dto, string Id);

public record UpdateBrokerFeatureResponse();

public class UpdateBrokerFeature(
    UserManager<User> userManager,
    IDocumentSession documentSession
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

        var broker = await documentSession.Query<Models.Broker>()
            .Where(x => x.AccessList.Any(y => y.UserId == user.Id))
            .Where(x => x.Id == request.Id)
            .SingleAsync();

        var access = broker.AccessList.Single(x => x.UserId == user.Id);
        access.Settings = new BrokerSettings()
        {
            QuickSearches = request.Dto.Settings.QuickSearches,
            DeadLetterQueueSuffix = request.Dto.Settings.DeadLetterQueueSuffix,
            MessageFormat = request.Dto.Settings.MessageFormat,
            MessageStructure = request.Dto.Settings.MessageStructure,
            QueueTrimPrefix = request.Dto.Settings.QueueTrimPrefix,
            DefaultQueueSortField = request.Dto.Settings.DefaultQueueSortField,
            DefaultQueueSortOrder = request.Dto.Settings.DefaultQueueSortOrder,
            DefaultQueueSearch = request.Dto.Settings.DefaultQueueSearch
        };

        var patch = documentSession.Patch<Models.Broker>(broker.Id);
        patch.Set(x => x.Name, request.Dto.Name);
        patch.Set(b => b.AccessList, broker.AccessList);
        patch.Set(b => b.UpdatedAt, DateTime.UtcNow);

        if (broker.AccessList
            .Any(x => x.UserId == user.Id &&
                      x.AccessLevel is AccessLevel.Owner or AccessLevel.Manager))
        {
            if (request.Dto.PostgresConnection is { } postgresConnection)
            {
                if (!string.IsNullOrEmpty(postgresConnection.Username) &&
                    !string.IsNullOrEmpty(postgresConnection.Password))
                {
                    patch.Set(b => b.PostgresConnection!.Username, postgresConnection.Username);
                    patch.Set(b => b.PostgresConnection!.Password, postgresConnection.Password);
                }

                patch.Set(b => b.PostgresConnection!.Host, postgresConnection.Host);
                patch.Set(b => b.PostgresConnection!.Database, postgresConnection.Database);
                patch.Set(b => b.PostgresConnection!.Port, postgresConnection.Port);
            }
        }

        await documentSession.SaveChangesAsync();

        return OperationResult<UpdateBrokerFeatureResponse>.Success(new UpdateBrokerFeatureResponse());
    }
}