using Marten;
using Marten.Patching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResQueue.Constants;
using ResQueue.Dtos.Stripe;
using ResQueue.Models;
using Stripe;
using Subscription = ResQueue.Models.Subscription;
using SubscriptionItem = ResQueue.Models.SubscriptionItem;

namespace ResQueue.Features.Stripe.CreateSubscription;

public record CreateSubscriptionRequest(
    string UserId,
    CreateSubscriptionDto Dto
);

public record CreateSubscriptionResponse();

public class CreateSubscriptionFeature(
    IDocumentSession documentSession,
    IOptions<Settings> settings
) : ICreateSubscriptionFeature
{
    public async Task<OperationResult<CreateSubscriptionResponse>> ExecuteAsync(CreateSubscriptionRequest request)
    {
        var dt = DateTime.UtcNow;

        var dc = new Dictionary<string, string>
        {
            { StripePlans.ESSENTIALS, settings.Value.StripeEssentialsPriceId },
            { StripePlans.ULTIMATE, settings.Value.StripeUltimatePriceId }
        };

        if (!dc.TryGetValue(request.Dto.Plan.ToLower(), out var priceId))
        {
            return OperationResult<CreateSubscriptionResponse>.Failure(new ProblemDetails
            {
                Title = "Invalid Subscription Plan",
                Detail = "The selected subscription plan is not valid.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var user = await documentSession.Query<User>().Where(x => x.Id == request.UserId).SingleAsync();

        if (user.Subscription?.StripeStatus == "active")
        {
            return OperationResult<CreateSubscriptionResponse>.Failure(new ProblemDetails
            {
                Title = "Subscription Already Active",
                Detail = "The user is already subscribed to an active plan.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var stripeClient = new StripeClient(settings.Value.StripeSecret);

            var customerOptions = new CustomerCreateOptions
            {
                Email = request.Dto.CustomerEmail,
                PaymentMethod = request.Dto.PaymentMethodId,
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = request.Dto.PaymentMethodId,
                },
            };
            var customer = await stripeClient.V1.Customers.CreateAsync(customerOptions);

            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = customer.Id,
                Coupon = request.Dto.Coupon,
                Items =
                [
                    new SubscriptionItemOptions
                    {
                        Price = priceId
                    }
                ],
                Expand = ["latest_invoice.payment_intent"],
            };

            var subscription = await stripeClient.V1.Subscriptions.CreateAsync(subscriptionOptions);

            var paymentMethod = await stripeClient.V1.PaymentMethods.GetAsync(request.Dto.PaymentMethodId);

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

            var patch = documentSession.Patch<User>(user.Id);
            patch.Set(q => q.StripeId, customer.Id);
            patch.Set(q => q.PaymentType, paymentType);
            patch.Set(q => q.PaymentLastFour, paymentLastFour);
            patch.Set(q => q.Subscription, user.Subscription);

            await documentSession.SaveChangesAsync();

            return OperationResult<CreateSubscriptionResponse>.Success(new CreateSubscriptionResponse());
        }
        catch (StripeException e)
        {
            return OperationResult<CreateSubscriptionResponse>.Failure(new ProblemDetails
            {
                Title = "Stripe Error",
                Detail = $"An error occurred while creating the subscription: {e.Message}",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}