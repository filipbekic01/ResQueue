using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Models;
using Stripe;
using Subscription = Resqueue.Models.Subscription;

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
            var subscription = await subscriptionService.UpdateAsync(request.SubscriptionId,
                new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                });

            // Update the subscription status in the user's document
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(q => q.Id, ObjectId.Parse(request.UserId)),
                Builders<User>.Filter.ElemMatch(q => q.Subscriptions,
                    Builders<Subscription>.Filter.Eq(s => s.StripeId, request.SubscriptionId))
            );

            var update = Builders<User>.Update
                .Set(q => q.Subscriptions[-1].StripeStatus,
                    subscription.Status) // -1 refers to the matched array element
                .Set(q => q.Subscriptions[-1].EndsAt,
                    subscription.CurrentPeriodEnd);

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