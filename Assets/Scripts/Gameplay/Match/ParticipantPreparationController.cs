using System;

public sealed class ParticipantPreparationController : IDisposable
{
    private readonly ParticipantAbilitySelectionRuntime abilitySelectionRuntime;
    private readonly ParticipantReadyStatusHandler readyStatusHandler;
    private readonly InputReader inputReader;
    private readonly ParticipantGameplayInputBinding gameplayInputBinding;

    public ParticipantPreparationController(
        ParticipantAbilitySelectionRuntime abilitySelectionRuntime,
        ParticipantReadyStatusHandler readyStatusHandler,
        InputReader inputReader)
    {
        this.abilitySelectionRuntime = abilitySelectionRuntime;
        this.readyStatusHandler = readyStatusHandler;
        this.inputReader = inputReader;
        gameplayInputBinding = inputReader != null
            ? new ParticipantGameplayInputBinding(inputReader)
            : null;

        abilitySelectionRuntime.BindInputReader(inputReader);
        readyStatusHandler.BindInputReader(inputReader);
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
        inputReader?.ApplyInputMode(PlayerInputMode.Disabled);
    }

    public void Tick(float deltaTime)
    {
        abilitySelectionRuntime.Tick(deltaTime);
    }

    public void BindAbilityController(PlayerAbilityController abilityController)
    {
        abilitySelectionRuntime.BindAbilityController(abilityController);
    }

    public void BindGameplayInputTargets(PlayerStrikerMovement movement, PlayerAbilityController abilityController)
    {
        gameplayInputBinding?.Bind(movement, abilityController);
    }

    public void ClearGameplayInputTargets()
    {
        gameplayInputBinding?.Unbind();
    }

    public void ApplyInputMode(PlayerInputMode inputMode)
    {
        inputReader?.ApplyInputMode(inputMode);
    }

    public void Dispose()
    {
        Disable();
        gameplayInputBinding?.Dispose();
        abilitySelectionRuntime.BindInputReader(null);
        readyStatusHandler.BindInputReader(null);
        inputReader?.Dispose();
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
