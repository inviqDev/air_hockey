using System;

public sealed class ParticipantPreparationController : IDisposable
{
    private readonly ParticipantAbilitySelectionRuntime abilitySelectionRuntime;
    private readonly ParticipantReadyStatusHandler readyStatusHandler;
    private readonly HumanPreparationCommandSource preparationCommandSource;

    public ParticipantPreparationController(
        ParticipantAbilitySelectionRuntime abilitySelectionRuntime,
        ParticipantReadyStatusHandler readyStatusHandler,
        HumanPreparationCommandSource preparationCommandSource)
    {
        this.abilitySelectionRuntime = abilitySelectionRuntime;
        this.readyStatusHandler = readyStatusHandler;
        this.preparationCommandSource = preparationCommandSource;

        abilitySelectionRuntime.BindPreparationCommandSource(preparationCommandSource);
        readyStatusHandler.BindPreparationCommandSource(preparationCommandSource);
    }

    public void Enable()
    {
        abilitySelectionRuntime.Enable();
        readyStatusHandler.Enable();
    }

    public void Disable()
    {
        readyStatusHandler.Disable();
        abilitySelectionRuntime.Disable();
        preparationCommandSource?.Disable();
    }

    public void Tick(float deltaTime)
    {
        abilitySelectionRuntime.Tick(deltaTime);
    }

    public void BindAbilityController(PlayerAbilityController abilityController)
    {
        abilitySelectionRuntime.BindAbilityController(abilityController);
    }

    public void ApplyInputMode(PlayerInputMode inputMode)
    {
        if (preparationCommandSource == null) return;

        if (inputMode == PlayerInputMode.Preparation)
        {
            preparationCommandSource.Enable();
            return;
        }

        preparationCommandSource.Disable();
    }

    public void Dispose()
    {
        Disable();
        abilitySelectionRuntime.BindPreparationCommandSource(null);
        readyStatusHandler.BindPreparationCommandSource(null);
        preparationCommandSource?.Dispose();
    }

    public void ResetProgression()
    {
        abilitySelectionRuntime.ResetProgression();
    }

    public void StartTurnProgression()
    {
        abilitySelectionRuntime.StartTurnProgression();
    }

    public void StopTurnProgression()
    {
        abilitySelectionRuntime.StopTurnProgression();
    }
}
