using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Models;

namespace ResQueue.Endpoints;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("test");

        group.MapGet("get-500", () => { throw new Exception("get500exception"); });

        group.MapPost("post-500", () => { throw new Exception("post500exception"); });

        group.MapGet("msgs", async (IMongoCollection<Broker> brokersCollection, IHostEnvironment env) =>
        {
            if (!env.IsDevelopment())
            {
                return;
            }

            // var broker = await brokersCollection
            //     .Find(Builders<Broker>.Filter.Eq(b => b.Id, ObjectId.Parse("66ffde2de31c63ef6436f1a5")))
            //     .SingleAsync();

            var broker = new Broker()
            {
                Id = new ObjectId(),
                CreatedByUserId = new ObjectId(),
                AccessList = [],
                System = "rmq",
                Name = "test",
                RabbitMQConnection = new RabbitMQConnection
                {
                    Host = "adorable-teal-antelope.rmq2.cloudamqp.com",
                    AmqpPort = 5671,
                    AmqpTls = true,
                    ManagementPort = 0,
                    ManagementTls = false,
                    Password = "hlxqVtwUbDYDUFxEgH3GuKBgKo0fLWy4",
                    Username = "luqrhkdv",
                    VHost = "luqrhkdv"
                }
            };

            var factory = RabbitmqConnectionFactory.CreateAmqpFactory(broker);
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var props = channel.CreateBasicProperties();

            for (int i = 0; i < 500; i++)
            {
                if (i is 100 or 200 or 300 or 400)
                {
                    await Task.Delay(1000);
                }

                channel.BasicPublish("hangfire_error", "", false, props, "message number"u8.ToArray());
            }
        });
    }
}