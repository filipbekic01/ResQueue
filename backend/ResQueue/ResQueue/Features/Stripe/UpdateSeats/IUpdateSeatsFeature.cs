namespace ResQueue.Features.Stripe.UpdateSeats;

public interface IUpdateSeatsFeature
{
    Task<OperationResult<UpdateSeatsResponse>> ExecuteAsync(UpdateSeatsRequest request);
}