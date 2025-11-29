using MassTransit;

namespace WebSample;

public record CustomExampleTestMessage(Guid CorrelationId) : CorrelatedBy<Guid>;