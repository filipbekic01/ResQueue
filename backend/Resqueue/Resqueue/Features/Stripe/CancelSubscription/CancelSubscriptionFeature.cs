using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Models;
using Stripe;

namespace Resqueue.Features.Stripe.CancelSubscription;

public record CancelSubscriptionRequest(
    string UserId,
    string SubscriptionId
);

public record CancelSubscriptionResponse();

public class CancelSubscriptionFeature(
    IMongoCollection<User> usersCollection,
    IOptions<Settings> settings
) : ICancelSubscriptionFeature
{
    public async Task<OperationResult<CancelSubscriptionResponse>> ExecuteAsync(CancelSubscriptionRequest request)
    {
        StripeConfiguration.ApiKey = settings.Value.StripeSecret;

        try
        {
            var subscriptionService = new SubscriptionService();
            await subscriptionService.CancelAsync(request.SubscriptionId);

            var filter = Builders<User>.Filter.Eq(q => q.Id, ObjectId.Parse(request.UserId));
            var update = Builders<User>.Update
                .Set(q => q.SubscriptionId, null)
                .Set(q => q.SubscriptionPlan, null)
                .Set(q => q.IsSubscribed, false);

            await usersCollection.UpdateOneAsync(filter, update);

            return OperationResult<CancelSubscriptionResponse>.Success(new CancelSubscriptionResponse());
        }
        catch (StripeException e)
        {
            return OperationResult<CancelSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = e.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}