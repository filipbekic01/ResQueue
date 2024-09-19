using System.Net.Http.Headers;
using System.Text;
using RabbitMQ.Client;
using ResQueue.Models;

namespace ResQueue;

public static class RabbitmqConnectionFactory
{
    public static ConnectionFactory CreateAmqpFactory(Broker broker)
    {
        if (broker.RabbitMQConnection is not { } rabbitMqConnection)
        {
            throw new Exception("RabbitMQ connection is missing.");
        }

        var connectionFactory = new ConnectionFactory()
        {
            HostName = rabbitMqConnection.Host,
            Port = rabbitMqConnection.AmqpPort,
            UserName = rabbitMqConnection.Username,
            Password = rabbitMqConnection.Password,
            VirtualHost = rabbitMqConnection.VHost,
        };

        if (rabbitMqConnection.AmqpTls)
        {
            connectionFactory.Ssl = new SslOption(rabbitMqConnection.Host)
            {
                Enabled = true,
            };
        }

        return connectionFactory;
    }

    public static HttpClient CreateManagementClient(IHttpClientFactory httpClientFactory, Broker broker)
    {
        if (broker.RabbitMQConnection is not { } rabbitMqConnection)
        {
            throw new Exception("RabbitMQ connection is missing.");
        }

        var client = httpClientFactory.CreateClient();

        var scheme = rabbitMqConnection.ManagementTls ? "https" : "http";
        client.BaseAddress = new Uri($"{scheme}://{rabbitMqConnection.Host}:{rabbitMqConnection.ManagementPort}");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{rabbitMqConnection.Username}:{rabbitMqConnection.Password}")));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }
}