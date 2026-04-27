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

    public void BeginPreparation()
    {
        EnsureTransition(FleetState.Docked, FleetState.Preparing);
        State = FleetState.Preparing;
    }

    public void MarkReady()
    {
        EnsureTransition(FleetState.Preparing, FleetState.Ready);
        State = FleetState.Ready;
    }

    public void FailPreparation()
    {
        EnsureTransition(FleetState.Preparing, FleetState.FailedPreparation);
        State = FleetState.FailedPreparation;
    }

    public void Deploy()
    {
        EnsureTransition(FleetState.Ready, FleetState.Deployed);
        State = FleetState.Deployed;
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
}