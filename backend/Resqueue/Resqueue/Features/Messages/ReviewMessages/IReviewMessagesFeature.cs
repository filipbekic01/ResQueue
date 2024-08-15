namespace Resqueue.Features.Messages.ReviewMessages;

public interface IReviewMessagesFeature
{
    Task<OperationResult<ReviewMessagesFeatureResponse>> ExecuteAsync(ReviewMessagesFeatureRequest request);
}