namespace ResQueue.Dtos.Stripe;

public record SubscribeDto(
    string PaymentMethodId,
    string Plan
);