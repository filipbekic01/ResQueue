using MassTransit;
using MassTransit.Scheduling;

namespace WebSample;

public class AwesomeSchedule : DefaultRecurringSchedule
{
    public AwesomeSchedule()
    {
        CronExpression = "0 0 31 2 *";
        TimeZoneId = TimeZoneInfo.Utc.Id;
    }
}

public record AwesomeRequest();

public class AwesomeConsumer() : IJobConsumer<AwesomeRequest>
{
    public Task Run(JobContext<AwesomeRequest> context)
    {
        Console.WriteLine("job finished!");

        return Task.CompletedTask;
    }
}