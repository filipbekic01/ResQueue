using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;

namespace Resqueue.Features.Messages.ReviewMessages;

public record ReviewMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, ReviewMessagesDto Dto);

public record ReviewMessagesFeatureResponse();

public class ReviewMessagesFeature(
    UserManager<User> userManager,
    IMongoCollection<Message> messagesCollection
) : IReviewMessagesFeature
{
    public async Task<OperationResult<ReviewMessagesFeatureResponse>> ExecuteAsync(ReviewMessagesFeatureRequest request)
    {
        var dt = DateTime.UtcNow;
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<ReviewMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "User not found"
            });
        }

        var bulkOperations = new List<WriteModel<Message>>();

        var filterToTrue = Builders<Message>.Filter.And(
            Builders<Message>.Filter.In(m => m.Id, request.Dto.IdsToTrue.Select(ObjectId.Parse)),
            Builders<Message>.Filter.Eq(m => m.UserId, user.Id)
        );
        var updateToTrue = Builders<Message>.Update
            .Set(m => m.UpdatedAt, dt)
            .Set(m => m.IsReviewed, true);
        bulkOperations.Add(new UpdateManyModel<Message>(filterToTrue, updateToTrue));

        var filterToFalse = Builders<Message>.Filter.And(
            Builders<Message>.Filter.In(m => m.Id, request.Dto.IdsToFalse.Select(ObjectId.Parse)),
            Builders<Message>.Filter.Eq(m => m.UserId, user.Id)
        );
        var update = Builders<Message>.Update
            .Set(m => m.UpdatedAt, dt)
            .Set(m => m.IsReviewed, false);
        bulkOperations.Add(new UpdateManyModel<Message>(filterToFalse, update));

        if (bulkOperations.Count != 0)
        {
            await messagesCollection.BulkWriteAsync(bulkOperations);
        }

        return OperationResult<ReviewMessagesFeatureResponse>.Success(new ReviewMessagesFeatureResponse());
    }
}