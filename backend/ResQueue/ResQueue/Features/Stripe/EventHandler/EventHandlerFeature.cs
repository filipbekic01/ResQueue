using Marten;
using Marten.Patching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResQueue.Models;
using Stripe;
using Subscription = Stripe.Subscription;

namespace ResQueue.Features.Stripe.EventHandler;

public record EventHandlerRequest(string JsonBody, string Signature);

public record EventHandlerResponse();

public class EventHandlerFeature(
    IOptions<Settings> settings,
    IDocumentSession documentSession
) : IEventHandlerFeature
{
    public async Task<OperationResult<EventHandlerResponse>> ExecuteAsync(EventHandlerRequest request)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                request.JsonBody,
                request.Signature,
                settings.Value.StripeSecretWebhook
            );

            switch (stripeEvent.Type)
            {
                case EventTypes.CustomerSubscriptionDeleted or EventTypes.CustomerSubscriptionUpdated:
                {
                    var subscription = stripeEvent.Data.Object as Subscription;

                    var user = await documentSession.Query<User>().Where(x => x.StripeId == subscription.CustomerId)
                        .FirstOrDefaultAsync();
                    if (user is null)
                    {
                        return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
                        {
                            Title = "Unauthorized Access",
                            Detail = "The user could not be found or is not authorized.",
                            Status = StatusCodes.Status401Unauthorized
                        });
                    }

                    if (user.Subscription?.StripeId != subscription.Id)
                    {
                        return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
                        {
                            Title = "Invalid Subscription",
                            Detail = "The subscription does not match the user's records.",
                            Status = StatusCodes.Status400BadRequest
                        });
                    }

                    user.Subscription.StripeStatus = subscription.Status;

                    documentSession.Patch<User>(user.Id)
                        .Set(x => x.Subscription, user.Subscription);

                    await documentSession.SaveChangesAsync();

                    break;
                }
                case EventTypes.InvoicePaymentFailed or EventTypes.InvoicePaymentSucceeded:
                {
                    var invoice = stripeEvent.Data.Object as Invoice;
                    var subscriptionId = invoice?.SubscriptionId;
                    var subscriptionService = new SubscriptionService();
                    var subscription = await subscriptionService.GetAsync(subscriptionId);

                    var user = await documentSession.Query<User>().Where(x => x.StripeId == subscription.CustomerId)
                        .FirstOrDefaultAsync();
                    if (user is not null)
                    {
                        if (user.Subscription is not null)
                        {
                            user.Subscription.StripeStatus = subscription.Status;

                            documentSession.Patch<User>(user.Id)
                                .Set(x => x.Subscription.StripeStatus, user.Subscription.StripeStatus);

                            await documentSession.SaveChangesAsync();
                        }
                        else
                        {
                            return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
                            {
                                Title = "Subscription Not Found",
                                Detail = "No subscription found for provided user.",
                                Status = StatusCodes.Status404NotFound
                            });
                        }
                    }
                    else
                    {
                        return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
                        {
                            Title = "User Not Found",
                            Detail = "No user found with the provided Stripe customer ID.",
                            Status = StatusCodes.Status404NotFound
                        });
                    }

                    break;
                }
                case EventTypes.PaymentIntentSucceeded or EventTypes.PaymentIntentPaymentFailed:
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    var subscriptionId = paymentIntent?.Metadata["subscription_id"];
                    var subscriptionService = new SubscriptionService();
                    var subscription = await subscriptionService.GetAsync(subscriptionId);

                    var user = await documentSession.Query<User>().Where(x => x.StripeId == subscription.CustomerId)
                        .FirstOrDefaultAsync();
                    if (user is not null)
                    {
                        if (user.Subscription is not null)
                        {
                            user.Subscription.StripeStatus = subscription.Status;

                            documentSession.Patch<User>(user.Id)
                                .Set(x => x.Subscription.StripeStatus, user.Subscription.StripeStatus);

                            await documentSession.SaveChangesAsync();
                        }
                        else
                        {
                            return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
                            {
                                Title = "Subscription Not Found",
                                Detail = "No subscription found for provided user.",
                                Status = StatusCodes.Status404NotFound
                            });
                        }
                    }
                    else
                    {
                        return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
                        {
                            Title = "User Not Found",
                            Detail = "No user found with the provided Stripe customer ID.",
                            Status = StatusCodes.Status404NotFound
                        });
                    }

                    break;
                }
            }

            return OperationResult<EventHandlerResponse>.Success(new EventHandlerResponse());
        }
        catch (StripeException e)
        {
            return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
            {
                Title = "Stripe Error",
                Detail = $"An error occurred while processing the event: {e.Message}",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}