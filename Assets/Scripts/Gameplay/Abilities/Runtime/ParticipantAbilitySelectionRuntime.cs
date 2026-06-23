public sealed class ParticipantAbilitySelectionRuntime
{
    private readonly AbilityHudView abilityHud;
    private readonly ParticipantAbilityProgression progression;
    private readonly ParticipantAbilityOfferFlow offerFlow;

    private PlayerAbilityController abilityController;

    public ParticipantAbilitySelectionRuntime(
        AbilityHudView abilityHud,
        ParticipantAbilityProgression progression,
        ParticipantAbilityOfferFlow offerFlow)
    {
        this.abilityHud = abilityHud;
        this.progression = progression;
        this.offerFlow = offerFlow;
    }

    public void Enable()
    {
        offerFlow.Enable();
        RefreshHud();
    }

    public void Disable()
    {
        offerFlow.Disable();
        RefreshHud();
    }

    public void Tick(float deltaTime)
    {
        progression.Tick(deltaTime);
        offerFlow.Tick();
        RefreshHud();
    }

    public void BindAbilityController(PlayerAbilityController controller)
    {
        var controllerChanged = abilityController != controller;
        abilityController = controller;
        offerFlow.BindAbilityController(controller);

        if (controllerChanged || !abilityController)
            offerFlow.CloseMenu();

        RefreshHud();
    }

    public void StartTurnProgression()
    {
        progression.StartTurnProgression();
        offerFlow.CloseMenu();
        RefreshHud();
    }

    public void StopTurnProgression()
    {
        progression.StopTurnProgression();
        RefreshHud();
    }

    public void ResetProgression()
    {
        progression.ResetProgression();
        offerFlow.CloseMenu();
        RefreshHud();
    }

    private void RefreshHud()
    {
        if (!abilityHud) return;

        abilityHud.SetFreeAbilityTimerText(progression.FreeAbilityTimerText);
        abilityHud.SetAvailableAmount(progression.AvailableAbilityPoints);
        abilityHud.SetAbilityMenuButtonEnabled(offerFlow.CanOpenMenu);
    }
}
