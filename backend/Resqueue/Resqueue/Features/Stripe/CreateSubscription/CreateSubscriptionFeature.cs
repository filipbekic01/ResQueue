using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Constants;
using Resqueue.Dtos.Stripe;
using Resqueue.Models;
using Stripe;
using Subscription = Resqueue.Models.Subscription;
using SubscriptionItem = Resqueue.Models.SubscriptionItem;

namespace Resqueue.Features.Stripe.CreateSubscription;

public record CreateSubscriptionRequest(
    string UserId,
    CreateSubscriptionDto Dto
);

public record CreateSubscriptionResponse();

public class CreateSubscriptionFeature(
    IMongoCollection<User> usersCollection,
    IOptions<Settings> settings
) : ICreateSubscriptionFeature
{
    public async Task<OperationResult<CreateSubscriptionResponse>> ExecuteAsync(CreateSubscriptionRequest request)
    {
        var dt = DateTime.UtcNow;

        StripeConfiguration.ApiKey = settings.Value.StripeSecret;

        var dc = new Dictionary<string, string>
        {
            { StripePlans.ESSENTIALS, "price_1PpyCoKE6sxW2owa2SY4jGXp" },
            { StripePlans.ULTIMATE, "price_1PpyDFKE6sxW2owaWndg9Wxc" }
        };

        if (!dc.TryGetValue(request.Dto.Plan.ToLower(), out var priceId))
        {
            return OperationResult<CreateSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = "Invalid subscription plan",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var filter = Builders<User>.Filter.Eq(q => q.Id, ObjectId.Parse(request.UserId));
        var user = await usersCollection.Find(filter).SingleAsync();

        if (user.Subscription?.StripeStatus == "active")
        {
            return OperationResult<CreateSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = "User subscribed already",
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

            var paymentMethodService = new PaymentMethodService();
            var paymentMethod = await paymentMethodService.GetAsync(request.Dto.PaymentMethodId);

            var paymentType = paymentMethod.Card?.Brand;
            var paymentLastFour = paymentMethod.Card?.Last4;

            user.Subscription = new Subscription
            {
                StripeId = subscription.Id,
                StripeStatus = subscription.Status,
                StripePrice = priceId,
                Type = request.Dto.Plan.ToLower(),
                Quantity = subscription.Items.Data[0].Quantity,
                EndsAt = null,
                CreatedAt = dt,
                UpdatedAt = dt,
                SubscriptionItem = new SubscriptionItem
                {
                    StripeId = subscription.Items.First().Id,
                    StripeProduct = subscription.Items.First().Price.ProductId,
                    StripePrice = subscription.Items.First().Price.Id,
                    Quantity = subscription.Items.First().Quantity,
                    CreatedAt = dt,
                    UpdatedAt = dt
                }
            };

            var update = Builders<User>.Update
                .Set(q => q.StripeId, customer.Id)
                .Set(q => q.PaymentType, paymentType)
                .Set(q => q.PaymentLastFour, paymentLastFour)
                .Set(q => q.Subscription, user.Subscription);

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