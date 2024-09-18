using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos.Stripe;
using Resqueue.Models;
using Stripe;

namespace Resqueue.Features.Stripe.ChangeCard;

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
        StripeConfiguration.ApiKey = settings.Value.StripeSecret;

        try
        {
            // Get user
            var filter = Builders<User>.Filter.Eq(q => q.Id, ObjectId.Parse(request.UserId));
            var user = await usersCollection.Find(filter).FirstOrDefaultAsync();
            if (user == null || string.IsNullOrEmpty(user.StripeId))
            {
                return OperationResult<ChangeCardResponse>.Failure(new ProblemDetails
                {
                    Detail = "User not found or Stripe customer ID missing",
                    Status = StatusCodes.Status404NotFound
                });
            }

            // Attach the new payment method to the customer
            var paymentMethodService = new PaymentMethodService();
            var paymentMethodAttachOptions = new PaymentMethodAttachOptions
            {
                Customer = user.StripeId
            };
            await paymentMethodService.AttachAsync(request.Dto.PaymentMethodId, paymentMethodAttachOptions);

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
            var paymentMethod = await paymentMethodService.GetAsync(request.Dto.PaymentMethodId);
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
                Detail = e.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}