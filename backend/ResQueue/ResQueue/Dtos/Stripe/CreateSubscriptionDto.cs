namespace ResQueue.Dtos.Stripe;

public record CreateSubscriptionDto(
    string CustomerEmail,
    string PaymentMethodId,
    string Plan
);