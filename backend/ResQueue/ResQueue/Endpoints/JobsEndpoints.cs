using MassTransit;
using MassTransit.Contracts.JobService;

namespace ResQueue.Endpoints;

public static class JobsEndpoints
{
    public static void MapJobsEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("jobs");

        group.MapGet("/{jobId:guid}/state",
            async (IRequestClient<GetJobState> client, Guid jobId) =>
            {
                var state = await client.GetJobState(jobId);

                return Results.Ok(state);
            });
    }
}