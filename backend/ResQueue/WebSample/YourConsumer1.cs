using MassTransit;

namespace WebSample;

public class YourConsumer1 : IConsumer<YourMessage1>
{
    public async Task Consume(ConsumeContext<YourMessage1> context)
    {
        Console.WriteLine("i got here!");
        await Task.Delay(0);
    }
}