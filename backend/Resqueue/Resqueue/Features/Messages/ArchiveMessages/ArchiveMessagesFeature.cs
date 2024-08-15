using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;

namespace Resqueue.Features.Messages.ArchiveMessages;

public record ArchiveMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, string QueueId, ArchiveMessagesDto Dto);

public record ArchiveMessagesFeatureResponse();

public class ArchiveMessagesFeature(
    UserManager<User> userManager,
    IMongoCollection<Message> messagesCollection
) : IArchiveMessagesFeature
{
    public async Task<OperationResult<ArchiveMessagesFeatureResponse>> ExecuteAsync(
        ArchiveMessagesFeatureRequest request)
    {
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<ArchiveMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "User not found"
            });
        }

        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.In(m => m.Id, request.Dto.Ids.Select(ObjectId.Parse)),
            Builders<Message>.Filter.Eq(m => m.QueueId, ObjectId.Parse(request.QueueId)),
            Builders<Message>.Filter.Eq(m => m.UserId, user.Id)
        );

        var dt = DateTime.UtcNow;

        var update = Builders<Message>.Update.Set(m => m.DeletedAt, dt);

        await messagesCollection.UpdateManyAsync(filter, update);

        return OperationResult<ArchiveMessagesFeatureResponse>.Success(new ArchiveMessagesFeatureResponse());
    }
}