using ResQueue.Enums;

namespace ResQueue
{
    public class ResQueueOptions
    {
        public ResQueueSqlEngine SqlEngine { get; set; } = ResQueueSqlEngine.Postgres;
    }
}