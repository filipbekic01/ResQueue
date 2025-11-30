using MassTransit;

namespace WebSample.Consumers;

/// <summary>
/// Message to schedule a party in 3 days from now.
/// Used to test scheduled message delivery.
/// </summary>
public record SchedulePartyMessage(
    Guid PartyId,
    string PartyName,
    string Location,
    DateTime PartyDate
);

/// <summary>
/// Consumer that handles scheduled party messages.
/// This demonstrates scheduled message delivery (3 days delay).
/// </summary>
public class SchedulePartyConsumer : IConsumer<SchedulePartyMessage>
{
    public Task Consume(ConsumeContext<SchedulePartyMessage> context)
    {
        Console.WriteLine($"🎉 Party scheduled! ID: {context.Message.PartyId}");
        Console.WriteLine($"   Name: {context.Message.PartyName}");
        Console.WriteLine($"   Location: {context.Message.Location}");
        Console.WriteLine($"   Date: {context.Message.PartyDate:yyyy-MM-dd HH:mm}");

        return Task.CompletedTask;
    }
}
