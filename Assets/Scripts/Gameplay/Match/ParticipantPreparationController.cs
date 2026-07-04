public sealed class ParticipantPreparationController
{
    private readonly ParticipantAbilitySelectionRuntime abilitySelectionRuntime;
    private readonly ParticipantReadyStatusHandler readyStatusHandler;

    public ParticipantPreparationController(
        ParticipantAbilitySelectionRuntime abilitySelectionRuntime,
        ParticipantReadyStatusHandler readyStatusHandler)
    {
        this.abilitySelectionRuntime = abilitySelectionRuntime;
        this.readyStatusHandler = readyStatusHandler;
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
    }

    public void Tick(float deltaTime)
    {
        abilitySelectionRuntime.Tick(deltaTime);
    }

    public void BindAbilityController(PlayerAbilityController abilityController)
    {
        abilitySelectionRuntime.BindAbilityController(abilityController);
        readyStatusHandler.BindInputReader(abilityController ? abilityController.InputReader : null);
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
