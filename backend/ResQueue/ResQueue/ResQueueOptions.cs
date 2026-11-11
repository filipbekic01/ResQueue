using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Features.Messages.GetSingleMessage.Models;
using ResQueue.QueueDepth;

namespace ResQueue
{
    public class ResQueueOptions
    {
        public ResQueueSqlEngine SqlEngine { get; set; } = ResQueueSqlEngine.Postgres;
        public Func<MessageDeliveryDto, Dictionary<string, string>>? AppendAdditionalData { get; set; }
        
        public IList<Type> TransformerTypes { get; } = new List<Type>();

        /// <summary>
        /// Periodic queue depth sampling used by the queue depth history chart. Disabled by default.
        /// </summary>
        public QueueDepthHistoryOptions QueueDepthHistory { get; } = new();
        
        public void AddTransformer<T>() where T : AbstractMessageTransformer
        {
            TransformerTypes.Add(typeof(T));
        }
    }
}