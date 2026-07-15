using System;

public sealed class TemporaryMatchWorld
{
    private readonly RoundController roundController;

    private MatchConfiguration activeConfiguration;

    public bool HasAllRoundItemsActive =>
        roundController && roundController.HasAllRoundItemsActive;

    public TemporaryMatchWorld(RoundController roundController)
    {
        if (!roundController)
            throw new ArgumentNullException(nameof(roundController));

        this.roundController = roundController;
    }

    public bool Activate(MatchConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        roundController.ReturnRoundItemsToPoolForFullMatch();
        activeConfiguration = null;

        var hasAllRoundItemsActive = roundController.ActivateRoundItems(configuration);
        activeConfiguration = configuration;

        return hasAllRoundItemsActive;
    }

    public bool ResetRoundItemsForTurn()
    {
        EnsureActiveConfiguration();
        return roundController.ResetRoundItemsForTurn();
    }

    public bool RebuildRoundItemsForTurn()
    {
        var configuration = EnsureActiveConfiguration();
        return roundController.RebuildRoundItemsForTurn(configuration);
    }

    public void ReturnRoundItemsToPoolForFullMatch()
    {
        roundController.ReturnRoundItemsToPoolForFullMatch();
        activeConfiguration = null;
    }

    public void SetAbilityPauseState(bool isPaused)
    {
        roundController.SetAbilityPauseState(isPaused);
    }

    public PlayerAbilityController GetAbilityController(ParticipantId participantId)
    {
        var slotId = GetSlotForParticipant(participantId);
        return roundController.GetAbilityController(slotId);
    }

    public bool TryGetHumanGameplayTargets(
        ParticipantId participantId,
        out PlayerStrikerMovement movement,
        out PlayerAbilityController abilityController)
    {
        var slotId = GetSlotForParticipant(participantId);
        return roundController.TryGetHumanGameplayTargets(slotId, out movement, out abilityController);
    }

    private ArenaSlotId GetSlotForParticipant(ParticipantId participantId)
    {
        return EnsureActiveConfiguration().SlotAssignments.GetSlotForParticipant(participantId);
    }

    private MatchConfiguration EnsureActiveConfiguration()
    {
        return activeConfiguration ?? throw new InvalidOperationException(
            $"{nameof(TemporaryMatchWorld)} requires an active match configuration for this operation.");
    }
}
