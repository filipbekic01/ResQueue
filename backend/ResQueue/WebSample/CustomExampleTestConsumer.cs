using MassTransit;

namespace WebSample;

public class CustomExampleTestConsumer : IConsumer<CustomExampleTestMessage>
{
    public async Task Consume(ConsumeContext<CustomExampleTestMessage> context)
    {
        Console.WriteLine("im starting!");

        await context.ScheduleSend(TimeSpan.FromHours(6), context.Message);

        throw new NotImplementedException();
    }
}