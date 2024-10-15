using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Dtos.Stripe;
using ResQueue.Models;
using Stripe;

namespace ResQueue.Features.Stripe.UpdateSeats;

public record UpdateSeatsRequest(
    string UserId,
    UpdateSeatsDto Dto
);

public record UpdateSeatsResponse();

public class UpdateSeatsFeature(
    IMongoCollection<User> usersCollection,
    IOptions<Settings> settings
) : IUpdateSeatsFeature
{
    public async Task<OperationResult<UpdateSeatsResponse>> ExecuteAsync(UpdateSeatsRequest request)
    {
        var dt = DateTime.UtcNow;

        // Get user
        var userFilter = Builders<User>.Filter.Eq(q => q.Id, ObjectId.Parse(request.UserId));
        var user = await usersCollection.Find(userFilter).SingleAsync();
        if (user.Subscription is not { StripeStatus: "active" })
        {
            return OperationResult<UpdateSeatsResponse>.Failure(new ProblemDetails
            {
                Title = "No Active Subscription",
                Detail = "The user does not have an active subscription to update seats.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var stripeClient = new StripeClient(settings.Value.StripeSecret);

            // Retrieve the current subscription
            var subscription = await stripeClient.V1.Subscriptions.GetAsync(user.Subscription.StripeId);

            // Calculate the new quantity (current seats + additional seats)
            var currentQuantity = subscription.Items.First().Quantity;
            var newQuantity = currentQuantity + request.Dto.Seats;

            // Update the subscription with the new quantity (seats)
            var subscriptionUpdateOptions = new SubscriptionUpdateOptions
            {
                Items =
                [
                    new SubscriptionItemOptions
                    {
                        Id = subscription.Items.First().Id,
                        Quantity = newQuantity
                    }
                ]
            };

            await stripeClient.V1.Subscriptions.UpdateAsync(subscription.Id, subscriptionUpdateOptions);

            // Update the user document with the new seat count and subscription details
            var update = Builders<User>.Update
                .Set(q => q.Subscription!.Quantity, newQuantity)
                .Set(q => q.Subscription!.UpdatedAt, dt)
                .Set(q => q.Subscription!.SubscriptionItem.Quantity, newQuantity)
                .Set(q => q.Subscription!.SubscriptionItem.UpdatedAt, dt);

            await usersCollection.UpdateOneAsync(userFilter, update);

            return OperationResult<UpdateSeatsResponse>.Success(new UpdateSeatsResponse());
        }
        catch (StripeException e)
        {
            return OperationResult<UpdateSeatsResponse>.Failure(new ProblemDetails
            {
                Title = "Stripe Error",
                Detail = $"An error occurred while updating the subscription: {e.Message}",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}