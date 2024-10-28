using Microsoft.Extensions.Options;
using ResQueue.Enums;
using ResQueue.Factories;

namespace ResQueue.Migrations;

public class SqlMigrations(
    IDatabaseConnectionFactory connectionFactory,
    IOptions<ResQueueOptions> options
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var purgeQueueByIdCommand = connectionFactory.CreateCommand(GetPurgeQueueByIdCommand(), connection);
        await purgeQueueByIdCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var deleteMessageCommand = connectionFactory.CreateCommand(GetDeleteMessageCommand(), connection);
        await deleteMessageCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private string GetPurgeQueueByIdCommand() =>
        options.Value.SqlEngine switch
        {
            ResQueueSqlEngine.Postgres =>
                string.Format("""
                              DROP FUNCTION IF EXISTS {0}._resqueue_purge_queue_by_id(bigint);
                              CREATE OR REPLACE FUNCTION {0}._resqueue_purge_queue_by_id(queue_id bigint)
                                  RETURNS void
                                  LANGUAGE plpgsql
                              AS
                              $$
                              BEGIN
                                  WITH msgs AS (
                                      DELETE FROM {0}.message_delivery md
                                          USING (SELECT mdx.message_delivery_id
                                                 FROM {0}.message_delivery mdx
                                                 WHERE mdx.queue_id = _resqueue_purge_queue_by_id.queue_id) mds
                                          WHERE md.message_delivery_id = mds.message_delivery_id
                                          RETURNING md.transport_message_id)
                                  DELETE
                                  FROM {0}.message m
                                      USING msgs
                                  WHERE m.transport_message_id = msgs.transport_message_id
                                    AND NOT EXISTS(SELECT
                                                   FROM {0}.message_delivery md
                                                   WHERE md.transport_message_id = m.transport_message_id);
                              END;
                              $$;
                              """, options.Value.Schema),
            ResQueueSqlEngine.SqlServer =>
                string.Format("""
                              CREATE OR ALTER PROCEDURE {0}._ResQueue_PurgeQueueById
                                  @QueueId BIGINT
                              AS
                              BEGIN
                                  SET NOCOUNT ON;
                                  
                                  DECLARE @tempMsgs TABLE (TransportMessageId uniqueidentifier);
                              
                                  DELETE FROM {0}.MessageDelivery
                                  OUTPUT DELETED.TransportMessageId INTO @tempMsgs
                                  WHERE QueueId = @QueueId;
                              
                                  DELETE FROM {0}.message
                                  WHERE TransportMessageId IN (SELECT TransportMessageId FROM @tempMsgs)
                                  AND NOT EXISTS (
                                      SELECT 1
                                      FROM {0}.MessageDelivery md
                                      WHERE md.TransportMessageId = {0}.message.TransportMessageId
                                  );
                              END;
                              """, options.Value.Schema),
            _ => throw new ArgumentOutOfRangeException()
        };

    private string GetDeleteMessageCommand() =>
        options.Value.SqlEngine switch
        {
            ResQueueSqlEngine.Postgres =>
                string.Format("""
                              DROP FUNCTION IF EXISTS {0}._resqueue_delete_message(bigint);
                              CREATE OR REPLACE FUNCTION {0}._resqueue_delete_message(message_delivery_id bigint)
                                RETURNS void
                                LANGUAGE PLPGSQL
                              AS
                              $$
                              DECLARE
                                  v_transport_message_id  uuid;
                              BEGIN
                                  DELETE FROM {0}.message_delivery md
                                      WHERE md.message_delivery_id = _resqueue_delete_message.message_delivery_id
                                      AND md.lock_id IS NULL
                                      RETURNING md.transport_message_id INTO v_transport_message_id;
                              
                                  IF v_transport_message_id IS NOT NULL THEN
                                      DELETE FROM {0}.message m
                                          WHERE m.transport_message_id = v_transport_message_id
                                          AND NOT EXISTS(SELECT FROM {0}.message_delivery md WHERE md.transport_message_id = v_transport_message_id);
                                  END IF;
                              END;
                              $$;
                              """, options.Value.Schema),
            ResQueueSqlEngine.SqlServer =>
                string.Format("""
                              CREATE OR ALTER PROCEDURE {0}._ResQueue_DeleteMessage
                                 @messageDeliveryId bigint
                              AS
                              BEGIN
                                  SET NOCOUNT ON;
                              
                                  DECLARE @outTransportMessageId uniqueidentifier;
                              
                                  DECLARE @DeletedMessages TABLE (
                                      MessageDeliveryId bigint,
                                      TransportMessageId uniqueidentifier,
                                      QueueId bigint
                                  );
                              
                                  DELETE
                                  FROM {0}.MessageDelivery
                                  OUTPUT deleted.MessageDeliveryId, deleted.TransportMessageId, deleted.QueueId
                                  INTO @DeletedMessages
                                  WHERE MessageDeliveryId = @messageDeliveryId
                                  AND LockId IS NULL;
                              
                                  SELECT TOP 1 @outTransportMessageId = TransportMessageId
                                      FROM @DeletedMessages;
                              
                                  IF @outTransportMessageId IS NOT NULL
                                  BEGIN
                                      DELETE m
                                      FROM {0}.Message m
                                      WHERE m.TransportMessageId = @outTransportMessageId
                                      AND NOT EXISTS (SELECT 1 FROM {0}.MessageDelivery md WHERE md.TransportMessageId = @outTransportMessageId);
                                  END;
                              END;
                              """, options.Value.Schema),
            _ => throw new ArgumentOutOfRangeException()
        };
}