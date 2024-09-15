using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Models;

namespace Resqueue.Features.Messages.CloneMessage;

public record CloneMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, string Id);

public record CloneMessagesFeatureResponse();

public class CloneMessageFeature(
    UserManager<User> userManager,
    IMongoCollection<Message> messagesCollection
) : ICloneMessageFeature
{
    public async Task<OperationResult<CloneMessagesFeatureResponse>> ExecuteAsync(CloneMessagesFeatureRequest request)
    {
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<CloneMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Detail = "User not found"
            });
        }

        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(m => m.Id, ObjectId.Parse(request.Id)),
            Builders<Message>.Filter.Eq(m => m.UserId, user.Id)
        );

        var message = await messagesCollection.Find(filter).SingleOrDefaultAsync();
        if (message == null)
        {
            return OperationResult<CloneMessagesFeatureResponse>.Failure(new ProblemDetails()
            {
                Status = 404,
                Detail = "Message not found"
            });
        }

        message.Id = default;
        message.UpdatedAt = null;
        message.CreatedAt = DateTime.UtcNow;

        await messagesCollection.InsertOneAsync(message);

        return OperationResult<CloneMessagesFeatureResponse>.Success(new CloneMessagesFeatureResponse());
    }
}