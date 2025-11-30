using MassTransit;

namespace WebSample.Consumers;

/// <summary>
/// Message for ordering drinks at the party.
/// Goes directly to dead-letter if age verification fails.
/// </summary>
public record OrderDrinksMessage(
    Guid OrderId,
    string CustomerName,
    int CustomerAge,
    string[] DrinkNames
);

/// <summary>
/// Consumer that checks age and throws if under 18.
/// Uses default retry behavior - will retry quickly until exhausted, then dead-letter.
/// </summary>
public class DrinkOrderConsumer : IConsumer<OrderDrinksMessage>
{
    public Task Consume(ConsumeContext<OrderDrinksMessage> context)
    {
        var message = context.Message;

        Console.WriteLine($"🍹 Processing drink order from {message.CustomerName}...");
        Console.WriteLine($"   Order ID: {message.OrderId}");
        Console.WriteLine($"   Age: {message.CustomerAge}");
        Console.WriteLine($"   Drinks: {string.Join(", ", message.DrinkNames)}");

        if (message.CustomerAge < 18)
        {
            // This will cause retries until exhausted, then dead-letter
            throw new UnauthorizedAccessException(
                $"Age verification failed! {message.CustomerName} is {message.CustomerAge} years old. " +
                "Must be 18 or older to order alcoholic drinks. This order has been rejected.");
        }

        Console.WriteLine($"✅ Drink order approved for {message.CustomerName}!");
        return Task.CompletedTask;
    }
}
