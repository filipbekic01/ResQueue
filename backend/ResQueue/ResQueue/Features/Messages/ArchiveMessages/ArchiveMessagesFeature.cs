using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Dtos;
using ResQueue.Models;

namespace ResQueue.Features.Messages.ArchiveMessages;

public record ArchiveMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, ArchiveMessagesDto Dto);

public record ArchiveMessagesFeatureResponse();

public class ArchiveMessagesFeature(
    UserManager<User> userManager,
    IMongoCollection<Message> messagesCollection
) : IArchiveMessagesFeature
{
    public async Task<OperationResult<ArchiveMessagesFeatureResponse>> ExecuteAsync(
        ArchiveMessagesFeatureRequest request)
    {
        var dt = DateTime.UtcNow;

        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<ArchiveMessagesFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "You must be logged in to sync the broker data.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.In(m => m.Id, request.Dto.Ids.Select(ObjectId.Parse)),
            Builders<Message>.Filter.Eq(m => m.UserId, user.Id)
        );

        var update = Builders<Message>.Update.Set(m => m.DeletedAt, dt);

        await messagesCollection.UpdateManyAsync(filter, update);

        return OperationResult<ArchiveMessagesFeatureResponse>.Success(new ArchiveMessagesFeatureResponse());
    }
}