using MassTransit;
using MassTransit.Contracts.JobService;
using WebSample.Consumers;

namespace WebSample;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this WebApplication app)
    {
        // 1. Schedule a party in 3 days
        app.MapGet("/schedule-party",
            async (IMessageScheduler scheduler) =>
            {
                var partyDate = DateTime.UtcNow.AddDays(3);
                var message = new SchedulePartyMessage(
                    PartyId: Guid.NewGuid(),
                    PartyName: "Summer Beach Party",
                    Location: "Bondi Beach, Sydney",
                    PartyDate: partyDate
                );

                // Schedule message delivery in 3 days
                await scheduler.SchedulePublish(partyDate, message);

                return TypedResults.Ok(new
                {
                    Message = "Party scheduled!",
                    ScheduledFor = partyDate,
                    PartyDetails = message
                });
            });

        // 2. Send birthday invite (always fails - for testing requeue with 8h expiration)
        app.MapGet("/send-birthday-invite",
            async (ISendEndpointProvider sendEndpointProvider) =>
            {
                var endpoint = await sendEndpointProvider.GetSendEndpoint(
                    new Uri("queue:birthday-invite"));

                var message = new SendBirthdayInviteMessage(
                    InviteId: Guid.NewGuid(),
                    GuestName: "John Smith",
                    GuestEmail: "john.smith@example.com",
                    BirthdayPersonName: "Jane Doe",
                    PartyDateTime: DateTime.UtcNow.AddDays(7)
                );

                // Send with 8 hour TTL
                await endpoint.Send(message, ctx =>
                {
                    ctx.TimeToLive = TimeSpan.FromHours(8);
                });

                return TypedResults.Ok(new
                {
                    Message = "Birthday invite sent (will fail for testing)!",
                    TimeToLive = "8 hours",
                    InviteDetails = message
                });
            });

        // 3. Order drinks (under 18 - goes to dead-letter after retries)
        app.MapGet("/order-drinks",
            async (IPublishEndpoint endpoint) =>
            {
                var message = new OrderDrinksMessage(
                    OrderId: Guid.NewGuid(),
                    CustomerName: "Tommy Teen",
                    CustomerAge: 16, // Under 18 - will be rejected
                    DrinkNames: ["Mojito", "Piña Colada", "Margarita"]
                );

                await endpoint.Publish(message);

                return TypedResults.Ok(new
                {
                    Message = "Drink order submitted (will be rejected - under 18)!",
                    OrderDetails = message
                });
            });

        // 4. Start recurring weather check job (every 3 minutes)
        app.MapGet("/start-weather-check",
            async (IPublishEndpoint endpoint) =>
            {
                var request = new CheckWeatherRequest(
                    Location: "Sydney, Australia",
                    PartyDate: DateTime.UtcNow.AddDays(3)
                );

                var jobId = await endpoint.AddOrUpdateRecurringJob(
                    "PartyWeatherCheck",
                    request,
                    x => x.Every(minutes: 3)
                );

                return TypedResults.Ok(new
                {
                    Message = "Weather check job started!",
                    JobId = jobId,
                    Schedule = "Every 3 minutes",
                    Request = request
                });
            });

        // Cancel a recurring job
        app.MapGet("/cancel-job/{jobId:guid}",
            async (IPublishEndpoint endpoint, Guid jobId) =>
            {
                await endpoint.CancelJob(jobId, "User requested cancellation");

                return TypedResults.Ok(new { Message = "Job cancelled", JobId = jobId });
            });

        // Get job state
        app.MapGet("/job-state/{jobId:guid}",
            async (IRequestClient<GetJobState> client, Guid jobId) =>
            {
                var state = await client.GetJobState(jobId);

                return TypedResults.Ok(state);
            });
    }
}
