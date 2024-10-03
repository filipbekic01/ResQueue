using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
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
    IMongoCollection<User> usersCollection,
    IOptions<Settings> settings
) : IChangeCardFeature
{
    public async Task<OperationResult<ChangeCardResponse>> ExecuteAsync(ChangeCardRequest request)
    {
        try
        {
            // Get user
            var filter = Builders<User>.Filter.Eq(q => q.Id, ObjectId.Parse(request.UserId));
            var user = await usersCollection.Find(filter).FirstOrDefaultAsync();
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

            // Update MongoDB with the new card details
            var update = Builders<User>.Update
                .Set(q => q.PaymentType, paymentType)
                .Set(q => q.PaymentLastFour, paymentLastFour);

            await usersCollection.UpdateOneAsync(filter, update);

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