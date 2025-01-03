using ResQueue.Dtos.Messages;

namespace ResQueue.Features.Messages.GetSingleMessage.Models
{
    public abstract class AbstractMessageTransformer
    {
        public virtual Task<MessageDeliveryDto> TransformAsync(MessageDeliveryDto message)
        {
            return Task.FromResult(message);
        }
    }
}