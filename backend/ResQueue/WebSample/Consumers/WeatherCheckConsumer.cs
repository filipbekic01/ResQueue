using MassTransit;

namespace WebSample.Consumers;

/// <summary>
/// Recurring job request to check weather for party planning.
/// Runs every 3 minutes.
/// </summary>
public record CheckWeatherRequest(
    string Location,
    DateTime PartyDate
);

/// <summary>
/// Job consumer that checks weather periodically for party planning.
/// Used to test recurring jobs (runs every 3 minutes).
/// </summary>
public class WeatherCheckConsumer : IJobConsumer<CheckWeatherRequest>
{
    private static readonly string[] WeatherConditions =
    [
        "☀️ Sunny",
        "⛅ Partly Cloudy",
        "☁️ Cloudy",
        "🌧️ Rainy",
        "⛈️ Thunderstorms",
        "🌈 Rainbow after rain"
    ];

    public Task Run(JobContext<CheckWeatherRequest> context)
    {
        var request = context.Job;
        var randomWeather = WeatherConditions[Random.Shared.Next(WeatherConditions.Length)];
        var temperature = Random.Shared.Next(15, 32);

        Console.WriteLine($"🌤️ Weather check for party planning:");
        Console.WriteLine($"   Location: {request.Location}");
        Console.WriteLine($"   Party Date: {request.PartyDate:yyyy-MM-dd}");
        Console.WriteLine($"   Current Weather: {randomWeather}");
        Console.WriteLine($"   Temperature: {temperature}°C");
        Console.WriteLine($"   Checked at: {DateTime.UtcNow:HH:mm:ss} UTC");

        return Task.CompletedTask;
    }
}
