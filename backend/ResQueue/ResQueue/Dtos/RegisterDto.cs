namespace ResQueue.Dtos;

public record RegisterDto(
    string Email,
    string Password,
    string PaymentMethodId,
    string Plan
);