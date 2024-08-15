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
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<ReviewMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "User not found"
            });
        }

        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.In(m => m.Id, request.Dto.Ids.Select(ObjectId.Parse)),
            Builders<Message>.Filter.Eq(m => m.UserId, user.Id)
        );

        var update = Builders<Message>.Update.Set(m => m.IsReviewed, true);

        await messagesCollection.UpdateManyAsync(filter, update);

        return OperationResult<ReviewMessagesFeatureResponse>.Success(new ReviewMessagesFeatureResponse());
    }
}