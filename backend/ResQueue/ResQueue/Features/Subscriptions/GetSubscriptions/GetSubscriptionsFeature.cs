using Dapper;
using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Factories;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue.Features.Subscriptions.GetSubscriptions;

public record GetSubscriptionsRequest();

public record GetSubscriptionsResponse(List<SubscriptionDto> Subscriptions);

public class GetSubscriptionsFeature(
    IDatabaseConnectionFactory connectionFactory,
    IDbConnectionProvider conn
) : IGetSubscriptionsFeature
{
    public async Task<OperationResult<GetSubscriptionsResponse>> ExecuteAsync(GetSubscriptionsRequest request)
    {
        await using var connection = connectionFactory.CreateConnection();

        var subscriptions = await connection.QueryAsync<SubscriptionDto>(GetSqlQueryText());

        return OperationResult<GetSubscriptionsResponse>.Success(new GetSubscriptionsResponse(
            Subscriptions: subscriptions.ToList()
        ));
    }

    private string GetSqlQueryText()
    {
        return conn.SqlEngine switch
        {
            ResQueueSqlEngine.Postgres => $"""
                                           SELECT 
                                               s.topic_name AS TopicName,
                                               s.destination_type AS DestinationType,
                                               s.destination_name AS DestinationName,
                                               s.subscription_type AS SubscriptionType,
                                               s.routing_key AS RoutingKey
                                           FROM {conn.Schema}.subscriptions s
                                           """,
            ResQueueSqlEngine.SqlServer => $"""
                                            SELECT 
                                                s.TopicName,
                                                s.DestinationType,
                                                s.DestinationName,
                                                s.SubscriptionType,
                                                s.RoutingKey
                                            FROM {conn.Schema}.Subscriptions s
                                            """,
            _ => throw new NotSupportedException("Unsupported SQL Engine")
        };
    }
}