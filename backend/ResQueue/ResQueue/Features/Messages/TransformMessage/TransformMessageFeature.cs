using Microsoft.Extensions.Options;
using ResQueue.Dtos.Messages;
using ResQueue.Features.Messages.TransformMessage.Models;

namespace ResQueue.Features.Messages.TransformMessage;

public record TransformMessageRequest(
    MessageDeliveryDto Message
);

public record TransformMessageResponse(
    MessageDeliveryDto Message
);

public class TransformMessageFeature(
    IServiceProvider serviceProvider,
    IOptions<ResQueueOptions> resQueueOptions
) : ITransformMessageFeature
{
    public async Task<OperationResult<TransformMessageResponse>> ExecuteAsync(TransformMessageRequest request)
    {
        var transformerTypes = resQueueOptions.Value.TransformerTypes;

        var message = request.Message;

        foreach (var transformerType in transformerTypes)
        {
            var transformer = (AbstractMessageTransformer)serviceProvider.GetRequiredService(transformerType);
            message = await transformer.TransformAsync(message);
        }

        return OperationResult<TransformMessageResponse>.Success(new TransformMessageResponse(message));
    }
}