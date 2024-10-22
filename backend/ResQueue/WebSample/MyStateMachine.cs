using MassTransit;

namespace WebSample;

public record EventOne(Guid CorrelationId) : CorrelatedBy<Guid>;

public record EventTwo(Guid CorrelationId) : CorrelatedBy<Guid>;

public class MyStateInstance : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public int CurrentState { get; set; }

    public string Name { get; set; }
}

public class MyStateDefinition : SagaDefinition<MyStateInstance>
{
    protected void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator)
    {
        if (endpointConfigurator is IServiceBusReceiveEndpointConfigurator sb)
        {
            sb.RequiresSession = true;
            sb.EnablePartitioning = true;
        }
    }
}

public class MyStateMachine : MassTransitStateMachine<MyStateInstance>
{
    public MyStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(EventOne)
                .Then((_) =>
                {
                    var x = 5;
                })
                .TransitionTo(Submitted)
        );

        During(Submitted,
            When(EventTwo)
                .ThenAsync(async (ctx) =>
                {
                    Console.WriteLine(
                        $"Consumed! and Guid: {ctx.Message.CorrelationId} and PKey: {ctx.PartitionKey()}");

                    await Task.Delay(TimeSpan.FromSeconds(3));
                }));
    }

    public Event<EventOne> EventOne { get; private set; }
    public Event<EventTwo> EventTwo { get; private set; }

    public State Submitted { get; private set; }
}