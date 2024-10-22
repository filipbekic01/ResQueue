using System.Diagnostics;
using MassTransit;

namespace WebSample;

public record YourMessage(Guid CorrelationId) : CorrelatedBy<Guid>;

public class YourConsumer : IConsumer<YourMessage>
{
    public async Task Consume(ConsumeContext<YourMessage> context)
    {
        Console.WriteLine(
            $"Consumed! Process ID: {Process.GetCurrentProcess().Id} and Guid: {context.Message.CorrelationId} and PKey: {context.PartitionKey()}");

        await Task.Delay(TimeSpan.FromSeconds(2));
        throw new Exception();
    }
}