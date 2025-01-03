using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Features.Messages.TransformMessage.Models;

namespace ResQueue
{
    public class ResQueueOptions
    {
        public ResQueueSqlEngine SqlEngine { get; set; } = ResQueueSqlEngine.Postgres;
        public Func<MessageDeliveryDto, Dictionary<string, string>>? AppendAdditionalData { get; set; }
        
        public IList<Type> TransformerTypes { get; } = new List<Type>();
        
        public void AddTransformer<T>() where T : AbstractMessageTransformer
        {
            TransformerTypes.Add(typeof(T));
        }
    }
}