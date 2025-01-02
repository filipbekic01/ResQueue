using ResQueue.Dtos.Messages;
using ResQueue.Enums;

namespace ResQueue
{
    public class ResQueueOptions
    {
        public ResQueueSqlEngine SqlEngine { get; set; } = ResQueueSqlEngine.Postgres;
        public Func<MessageDeliveryDto, Dictionary<string, string>>? AppendAdditionalData { get; set; }
    }
}