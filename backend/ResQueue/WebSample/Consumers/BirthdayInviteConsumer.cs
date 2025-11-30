using MassTransit;

namespace WebSample.Consumers;

/// <summary>
/// Message for sending birthday party invitations.
/// Has 8-hour TTL - used to test requeue mechanism on failures.
/// </summary>
public record SendBirthdayInviteMessage(
    Guid InviteId,
    string GuestName,
    string GuestEmail,
    string BirthdayPersonName,
    DateTime PartyDateTime
);

/// <summary>
/// Consumer that ALWAYS fails to test the requeue mechanism.
/// Messages have 8-hour expiration to test TTL behavior.
/// </summary>
public class BirthdayInviteConsumer : IConsumer<SendBirthdayInviteMessage>
{
    public Task Consume(ConsumeContext<SendBirthdayInviteMessage> context)
    {
        Console.WriteLine($"📧 Attempting to send birthday invite to {context.Message.GuestName}...");
        Console.WriteLine($"   Email: {context.Message.GuestEmail}");
        Console.WriteLine($"   Birthday Person: {context.Message.BirthdayPersonName}");
        Console.WriteLine($"   Party Date: {context.Message.PartyDateTime:yyyy-MM-dd HH:mm}");

        // Always fail to test requeue mechanism
        throw new InvalidOperationException(
            $"Failed to send birthday invite to {context.Message.GuestEmail}! " +
            "Email service is temporarily unavailable. Please try again later.");
    }
}
