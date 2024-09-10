using System.Net.Security;
using RabbitMQ.Client;
using Resqueue.Models;

namespace Resqueue;

public class RabbitmqConnectionFactory
{
    public ConnectionFactory CreateFactory(Broker broker) => new()
    {
        HostName = broker.Host,
        Port = 5671,
        UserName = broker.Username,
        Password = broker.Password,
        VirtualHost = broker.VHost,
        Ssl = new SslOption("resqueue")
        {
            AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch,
            Enabled = true,
        },
    };
}