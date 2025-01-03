using Microsoft.AspNetCore.Mvc;
using ResQueue.Dtos.Messages;
using ResQueue.Features.Messages.DeleteMessages;
using ResQueue.Features.Messages.GetMessages;
using ResQueue.Features.Messages.GetSingleMessage;
using ResQueue.Features.Messages.RequeueMessages;
using ResQueue.Features.Messages.RequeueSpecificMessages;

namespace ResQueue.Endpoints;

public static class MessagesEndpoints
{
    public static void MapMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("messages");

        group.MapGet("",
            async (IGetMessagesFeature feature, [FromQuery] long queueId, [FromQuery] int pageIndex = 0) =>
            {
                var result = await feature.ExecuteAsync(new GetMessagesRequest(
                    queueId, pageIndex
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value!.Messages)
                    : Results.Problem(result.Problem!);
            });

        group.MapGet("{transportMessageId}",
            async (IGetSingleMessageFeature feature, string transportMessageId) =>
            {
                var result = await feature.ExecuteAsync(new GetSingleMessageRequest(
                    transportMessageId
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value!.Message)
                    : Results.Problem(result.Problem!);
            });

        group.MapPost("requeue",
            async (IRequeueMessagesFeature feature, [FromBody] RequeueMessagesDto dto) =>
            {
                var result = await feature.ExecuteAsync(new RequeueMessagesRequest(
                    dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            });

        group.MapPost("requeue-specific",
            async (IRequeueSpecificMessagesFeature feature, [FromBody] RequeueSpecificMessagesDto dto) =>
            {
                var result = await feature.ExecuteAsync(new RequeueSpecificMessagesRequest(
                    dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            });

        group.MapDelete("", async (IDeleteMessagesFeature feature, [FromBody] DeleteMessagesDto dto) =>
        {
            var result = await feature.ExecuteAsync(new DeleteMessagesRequest(
                dto
            ));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Problem!);
        });
    }
}