using System.Security.Claims;
using Resqueue.Dtos;

namespace Resqueue.Features.Messages.ArchiveMessages;

public record ArchiveMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, string QueueId, ArchiveMessagesDto Dto);

public record ArchiveMessagesFeatureResponse();

public class ArchiveMessagesFeature : IArchiveMessagesFeature
{
    public async Task<OperationResult<ArchiveMessagesFeatureResponse>> ExecuteAsync(
        ArchiveMessagesFeatureRequest request)
    {
        await Task.Delay(0);

        return OperationResult<ArchiveMessagesFeatureResponse>.Success(new ArchiveMessagesFeatureResponse());
    }
}