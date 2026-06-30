using System;

public sealed class ParticipantAbilitySelectionRuntime
{
    private readonly ParticipantHudView participantHud;
    private readonly AbilityPointsProgression pointsProgression;
    private readonly AbilityOfferSelectionFlow offerSelectionFlow;

    private PlayerAbilityController abilityController;
    private bool hasRenderedHudState;
    private int lastRenderedTimerDisplaySeconds;
    private int lastRenderedAvailableAbilityPoints;
    private bool lastRenderedCanOpenMenu;

    public bool IsMenuOpen => offerSelectionFlow.IsMenuOpen;

    public event Action<bool> MenuOpenStateChanged
    {
        add => offerSelectionFlow.MenuOpenStateChanged += value;
        remove => offerSelectionFlow.MenuOpenStateChanged -= value;
    }

    public ParticipantAbilitySelectionRuntime(
        ParticipantHudView participantHud,
        AbilityPointsProgression pointsProgression,
        AbilityOfferSelectionFlow offerSelectionFlow)
    {
        this.participantHud = participantHud;
        this.pointsProgression = pointsProgression;
        this.offerSelectionFlow = offerSelectionFlow;
    }

    public void Enable()
    {
        offerSelectionFlow.Enable();
        InvalidateHudRenderCache();
        RefreshHud();
    }

    public void Disable()
    {
        offerSelectionFlow.Disable();
        InvalidateHudRenderCache();
        RefreshHud();
    }

    public void Tick(float deltaTime)
    {
        pointsProgression.Tick(deltaTime);
        offerSelectionFlow.Tick();
        RefreshHud();
    }

    public void BindAbilityController(PlayerAbilityController controller)
    {
        var controllerChanged = abilityController != controller;
        abilityController = controller;
        offerSelectionFlow.BindAbilityController(controller);

        if (controllerChanged)
            InvalidateHudRenderCache();

        RefreshHud();
    }

    public void StartTurnProgression()
    {
        pointsProgression.StartAbilityPointsTurnProgression();
        offerSelectionFlow.CloseMenu();
        InvalidateHudRenderCache();
        RefreshHud();
    }

    public void StopTurnProgression()
    {
        pointsProgression.StopAbilityPointsTurnProgression();
        InvalidateHudRenderCache();
        RefreshHud();
    }

    public void ResetProgression()
    {
        pointsProgression.ResetPointsProgression();
        offerSelectionFlow.CloseMenu();
        InvalidateHudRenderCache();
        RefreshHud();
    }

    private void RefreshHud()
    {
        if (!participantHud) return;

        var currentTimerDisplaySeconds = pointsProgression.FreeAbilityTimerDisplaySeconds;
        var currentAvailableAbilityPoints = pointsProgression.AvailableAbilityPoints;
        var currentCanOpenMenu = offerSelectionFlow.CanOpenMenu;

        if (!hasRenderedHudState || currentTimerDisplaySeconds != lastRenderedTimerDisplaySeconds)
        {
            participantHud.SetFreeAbilityTimerText(pointsProgression.FreeAbilityTimerText);
            lastRenderedTimerDisplaySeconds = currentTimerDisplaySeconds;
        }

        if (!hasRenderedHudState || currentAvailableAbilityPoints != lastRenderedAvailableAbilityPoints)
        {
            participantHud.SetAvailableAmount(currentAvailableAbilityPoints);
            lastRenderedAvailableAbilityPoints = currentAvailableAbilityPoints;
        }

        if (!hasRenderedHudState || currentCanOpenMenu != lastRenderedCanOpenMenu)
        {
            participantHud.SetAbilityMenuButtonEnabled(currentCanOpenMenu);
            lastRenderedCanOpenMenu = currentCanOpenMenu;
        }

        hasRenderedHudState = true;
    }

    private void InvalidateHudRenderCache()
    {
        hasRenderedHudState = false;
    }
}
