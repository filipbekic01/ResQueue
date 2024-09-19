namespace ResQueue.Features.Stripe.ChangeCard;

public interface IChangeCardFeature
{
    Task<OperationResult<ChangeCardResponse>> ExecuteAsync(ChangeCardRequest request);
}