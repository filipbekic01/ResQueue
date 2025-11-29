using MassTransit;

namespace WebSample;

public record YourMessage1(Guid CorrelationId) : CorrelatedBy<Guid>;