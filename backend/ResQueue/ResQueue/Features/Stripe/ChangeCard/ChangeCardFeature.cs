using Marten;
using Marten.Patching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResQueue.Dtos.Stripe;
using ResQueue.Models;
using Stripe;

namespace ResQueue.Features.Stripe.ChangeCard;

public record ChangeCardRequest(
    string UserId,
    ChangeCardDto Dto
);

public record ChangeCardResponse();

public class ChangeCardFeature(
    IDocumentSession documentSession,
    IOptions<Settings> settings
) : IChangeCardFeature
{
    public async Task<OperationResult<ChangeCardResponse>> ExecuteAsync(ChangeCardRequest request)
    {
        try
        {
            // Get user
            var user = await documentSession.Query<User>().Where(x => x.Id == request.UserId).SingleAsync();
            if (user == null || string.IsNullOrEmpty(user.StripeId))
            {
                return OperationResult<ChangeCardResponse>.Failure(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = "The user could not be found or the Stripe customer ID is missing.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            // Attach the new payment method to the customer
            var stripeClient = new StripeClient(settings.Value.StripeSecret);

            await stripeClient.V1.PaymentMethods.AttachAsync(request.Dto.PaymentMethodId, new PaymentMethodAttachOptions
            {
                Customer = user.StripeId
            });

            // Set the new payment method as the default
            var customerService = new CustomerService();
            var customerUpdateOptions = new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = request.Dto.PaymentMethodId
                }
            };
            await customerService.UpdateAsync(user.StripeId, customerUpdateOptions);

            // Get details about the new payment method (e.g., card brand and last four digits)
            var paymentMethod = await stripeClient.V1.PaymentMethods.GetAsync(request.Dto.PaymentMethodId);
            var paymentType = paymentMethod.Card?.Brand; // e.g., "visa", "mastercard"
            var paymentLastFour = paymentMethod.Card?.Last4; // Last four digits of the card

            var patch = documentSession.Patch<User>(user.Id);
            patch.Set(q => q.PaymentType, paymentType);
            patch.Set(q => q.PaymentLastFour, paymentLastFour);

            await documentSession.SaveChangesAsync();

            return OperationResult<ChangeCardResponse>.Success(new ChangeCardResponse());
        }
        catch (StripeException e)
        {
            return OperationResult<ChangeCardResponse>.Failure(new ProblemDetails
            {
                Title = "Payment Method Update Failed",
                Detail = $"An error occurred while updating the payment method: {e.Message}",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}