using Microsoft.AspNetCore.Mvc;
using Resqueue.Dtos.Stripe;
using Stripe;

namespace Resqueue.Features.Stripe.CreateSubscription;

public record CreateSubscriptionRequest(
    CreateSubscriptionDto Dto
);

public record CreateSubscriptionResponse(
    string SubscriptionId,
    string ClientSecret,
    bool RequiresAction
);

public class CreateSubscriptionFeature : ICreateSubscriptionFeature
{
    public async Task<OperationResult<CreateSubscriptionResponse>> ExecuteAsync(CreateSubscriptionRequest request)
    {
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
                        Price = request.Dto.PriceId,
                    }
                ],
                Expand = ["latest_invoice.payment_intent"],
            };

            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.CreateAsync(subscriptionOptions);

            var paymentIntent = subscription.LatestInvoice.PaymentIntent;

            return OperationResult<CreateSubscriptionResponse>.Success(new CreateSubscriptionResponse
            (
                SubscriptionId: subscription.Id,
                ClientSecret: paymentIntent.ClientSecret,
                RequiresAction: paymentIntent.Status == "requires_action"
            ));
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