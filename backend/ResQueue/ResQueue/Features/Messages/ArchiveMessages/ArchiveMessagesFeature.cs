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
    IMongoCollection<Message> messagesCollection,
    IMongoCollection<Queue> queuesCollection,
    IMongoClient mongoClient
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
            Builders<Message>.Filter.Eq(m => m.QueueId, ObjectId.Parse(request.Dto.QueueId)),
            Builders<Message>.Filter.Eq(m => m.UserId, user.Id)
        );

        var update = Builders<Message>.Update.Set(m => m.DeletedAt, dt);

        using var session = await mongoClient.StartSessionAsync();
        session.StartTransaction();

        var result = await messagesCollection.UpdateManyAsync(session, filter, update);

        await queuesCollection.UpdateOneAsync(session, x => x.Id == ObjectId.Parse(request.Dto.QueueId),
            Builders<Queue>.Update.Inc(x => x.TotalMessages, -result.ModifiedCount));

        await queuesCollection.UpdateOneAsync(session, x => x.Id == ObjectId.Parse(request.Dto.QueueId),
            Builders<Queue>.Update.Max(x => x.TotalMessages, 0));

        await session.CommitTransactionAsync();

        return OperationResult<ArchiveMessagesFeatureResponse>.Success(new ArchiveMessagesFeatureResponse());
    }
}