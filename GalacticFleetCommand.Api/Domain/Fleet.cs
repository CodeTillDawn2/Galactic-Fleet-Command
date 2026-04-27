using GalacticFleetCommand.Api.Domain.Exceptions;

namespace GalacticFleetCommand.Api.Domain;

public enum FleetState
{
    Docked,
    Preparing,
    Ready,
    Deployed,
    FailedPreparation
}

public class Fleet : IVersionedEntity
{
    public required string Id { get; init; }

    /// <summary>
    /// Version used for optimistic concurrency control.
    /// </summary>
    public int Version { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// Number of ships in the fleet.
    /// </summary>
    public int ShipCount { get; set; }

    /// <summary>
    /// Fuel required to prepare the fleet for deployment.
    /// </summary>
    public int FuelRequired { get; set; }

    public FleetState State { get; private set; } = FleetState.Docked;

    /// <summary>
    /// Recorded lifecycle state transitions for this fleet.
    /// </summary>
    public List<FleetStateTransition> Transitions { get; } = [];

    public void BeginPreparation()
    {
        EnsureTransition(FleetState.Docked, FleetState.Preparing);
        TransitionTo(FleetState.Preparing);
    }

    public void MarkReady()
    {
        EnsureTransition(FleetState.Preparing, FleetState.Ready);
        TransitionTo(FleetState.Ready);
    }

    public void FailPreparation()
    {
        EnsureTransition(FleetState.Preparing, FleetState.FailedPreparation);
        TransitionTo(FleetState.FailedPreparation);
    }

    public void Deploy()
    {
        EnsureTransition(FleetState.Ready, FleetState.Deployed);
        TransitionTo(FleetState.Deployed);
    }

    public void Dock()
    {
        EnsureTransition(FleetState.Deployed, FleetState.Docked);
        TransitionTo(FleetState.Docked);
    }

    private void EnsureTransition(FleetState expectedCurrentState, FleetState attemptedState)
    {
        if (State != expectedCurrentState)
        {
            throw new InvalidFleetStateTransitionException(
                currentState: State,
                attemptedState: attemptedState,
                expectedCurrentState: expectedCurrentState);
        }
    }

    private void TransitionTo(FleetState newState)
    {
        var previousState = State;

        State = newState;

        Transitions.Add(new FleetStateTransition
        {
            From = previousState,
            To = newState,
            OccurredAtUtc = DateTime.UtcNow
        });
    }
}