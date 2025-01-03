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

        // invoke each transformer
        foreach (var transformerType in transformerTypes)
        {
            var transformer = (AbstractMessageTransformer)serviceProvider.GetRequiredService(transformerType);
            message = await transformer.TransformAsync(message);
        }

        // append additional data
        if (resQueueOptions.Value.AppendAdditionalData is not null)
        {
            var additionalData = resQueueOptions.Value.AppendAdditionalData(message);
            message.AdditionalData = additionalData;
        }

        return OperationResult<TransformMessageResponse>.Success(new TransformMessageResponse(message));
    }
}