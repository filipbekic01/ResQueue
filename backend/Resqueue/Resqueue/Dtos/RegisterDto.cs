namespace Resqueue.Dtos;

public record RegisterDto(
    string Email,
    string Password,
    string PaymentMethodId,
    string PriceId
);