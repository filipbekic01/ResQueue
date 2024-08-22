using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos.Stripe;
using Resqueue.Models;
using Stripe;

namespace Resqueue.Features.Stripe.CreateSubscription;

public record CreateSubscriptionRequest(
    string UserId,
    CreateSubscriptionDto Dto
);

public record CreateSubscriptionResponse();

public class CreateSubscriptionFeature(
    IMongoCollection<User> usersCollection
) : ICreateSubscriptionFeature
{
    public async Task<OperationResult<CreateSubscriptionResponse>> ExecuteAsync(CreateSubscriptionRequest request)
    {
        StripeConfiguration.ApiKey =
            "sk_test_51PpxV4KE6sxW2owadyGKDtUF7cDjpCEtD83stbkfbzd7FqPfleW0pWKEmJR6BGr7oLnwIjRAQPCFxQsnMDYOr79h00cKaT8nO1";

        var dc = new Dictionary<string, string>
        {
            { "essentials", "price_1PpyCoKE6sxW2owa2SY4jGXp" },
            { "ultimate", "price_1PpyDFKE6sxW2owaWndg9Wxc" }
        };

        if (!dc.TryGetValue(request.Dto.Plan.ToLower(), out var priceId))
        {
            return OperationResult<CreateSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = "Invalid subscription plan",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var customerOptions = new CustomerCreateOptions
            {
                Email = request.Dto.CustomerEmail,
                PaymentMethod = request.Dto.PaymentMethodId,
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = request.Dto.PaymentMethodId,
                },
            };
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(customerOptions);

            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = customer.Id,
                Items =
                [
                    new SubscriptionItemOptions
                    {
                        Price = priceId
                    }
                ],
                Expand = ["latest_invoice.payment_intent"],
            };

            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.CreateAsync(subscriptionOptions);

            var filter = Builders<User>.Filter.Eq(q => q.Id, ObjectId.Parse(request.UserId));
            var update = Builders<User>.Update
                .Set(q => q.SubscriptionId, subscription.Id)
                .Set(q => q.SubscriptionPlan, request.Dto.Plan.ToLower())
                .Set(q => q.IsSubscribed, true);

            await usersCollection.UpdateOneAsync(filter, update);

            return OperationResult<CreateSubscriptionResponse>.Success(new CreateSubscriptionResponse());
        }
        catch (StripeException e)
        {
            return OperationResult<CreateSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = e.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}