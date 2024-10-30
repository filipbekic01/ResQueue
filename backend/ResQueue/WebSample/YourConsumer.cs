using System.Diagnostics;
using MassTransit;
using MassTransit.Middleware;

namespace WebSample;

public record YourMessage(Guid CorrelationId) : CorrelatedBy<Guid>;

public class YourConsumer : IConsumer<YourMessage>
{
    public async Task Consume(ConsumeContext<YourMessage> context)
    {
        Console.WriteLine("im starting!");

        await context.ScheduleSend(TimeSpan.FromHours(6), context.Message);

        throw new NotImplementedException();
    }
}