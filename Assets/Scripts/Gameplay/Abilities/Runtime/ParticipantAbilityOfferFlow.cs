using System;
using System.Collections.Generic;

public sealed class ParticipantAbilityOfferFlow
{
    private readonly AbilityHudView abilityHud;
    private readonly AbilitySelectionMenu abilitySelectionMenu;
    private readonly ParticipantAbilityProgression progression;
    private readonly AbilityOfferService offerService;
    private readonly AbilityCatalog abilityCatalog;
    private readonly Func<bool> isMenuInteractionAllowed;
    private readonly ParticipantAbilitySelectionSession selectionSession = new();

    private PlayerAbilityController abilityController;
    private PlayerInputReader inputReader;
    private bool isEnabled;

    public ParticipantAbilityOfferFlow(
        AbilityHudView abilityHud,
        AbilitySelectionMenu abilitySelectionMenu,
        ParticipantAbilityProgression progression,
        AbilityCatalog abilityCatalog,
        AbilityOfferService offerService,
        Func<bool> isMenuInteractionAllowed)
    {
        this.abilityHud = abilityHud;
        this.abilitySelectionMenu = abilitySelectionMenu;
        this.progression = progression;
        this.abilityCatalog = abilityCatalog;
        this.offerService = offerService;
        this.isMenuInteractionAllowed = isMenuInteractionAllowed;
    }

    public bool CanOpenMenu => CanOpenMenuInternal();

    public void Enable()
    {
        if (isEnabled) return;

        isEnabled = true;
        SubscribeToHud();
        SubscribeToMenu();
        SubscribeToInputReader();
    }

    public void Disable()
    {
        if (!isEnabled) return;

        CloseMenu();
        UnsubscribeFromInputReader();
        UnsubscribeFromMenu();
        UnsubscribeFromHud();
        isEnabled = false;
    }

    private void BindInputReader(PlayerInputReader nextInputReader)
    {
        if (inputReader == nextInputReader) return;

        if (isEnabled)
            UnsubscribeFromInputReader();

        inputReader = nextInputReader;

        if (isEnabled)
            SubscribeToInputReader();
    }

    public void BindAbilityController(PlayerAbilityController controller)
    {
        if (abilityController == controller) return;

        abilityController = controller;
        BindInputReader(abilityController ? abilityController.InputReader : null);
    }

    public void CloseMenu()
    {
        selectionSession.Close();

        if (!abilitySelectionMenu) return;
        abilitySelectionMenu.Close();
    }

    public void Tick()
    {
        if (selectionSession.State != ParticipantAbilitySelectionSession.SessionState.SelectingOffer) return;
        if (CanKeepMenuOpen()) return;

        CloseMenu();
    }

    private void SubscribeToHud()
    {
        if (!abilityHud) return;
        abilityHud.PlusAbilityButtonClicked += HandleMenuToggleRequested;
    }

    private void UnsubscribeFromHud()
    {
        if (!abilityHud) return;
        abilityHud.PlusAbilityButtonClicked -= HandleMenuToggleRequested;
    }

    private void SubscribeToMenu()
    {
        if (!abilitySelectionMenu) return;
        abilitySelectionMenu.OfferClicked += HandleOfferClicked;
    }

    private void UnsubscribeFromMenu()
    {
        if (!abilitySelectionMenu) return;
        abilitySelectionMenu.OfferClicked -= HandleOfferClicked;
    }

    private void SubscribeToInputReader()
    {
        if (!inputReader) return;

        inputReader.AbilitySelectionMenuPressed += HandleMenuToggleRequested;
        inputReader.AbilitySelectionPreviousPressed += HandlePreviousOfferRequested;
        inputReader.AbilitySelectionNextPressed += HandleNextOfferRequested;
    }

    private void UnsubscribeFromInputReader()
    {
        if (!inputReader) return;

        inputReader.AbilitySelectionMenuPressed -= HandleMenuToggleRequested;
        inputReader.AbilitySelectionPreviousPressed -= HandlePreviousOfferRequested;
        inputReader.AbilitySelectionNextPressed -= HandleNextOfferRequested;
    }

    private void HandleMenuToggleRequested()
    {
        if (selectionSession.State == ParticipantAbilitySelectionSession.SessionState.SelectingOffer)
        {
            CloseMenu();
            return;
        }

        if (!CanOpenMenuInternal()) return;

        var offers = BuildOffers();
        if (!selectionSession.TryOpen(offers)) return;

        RenderMenu();
    }

    private void HandleOfferClicked(int offerIndex)
    {
        if (!selectionSession.TrySelectOffer(offerIndex)) return;

        RenderMenu();
    }

    private void HandlePreviousOfferRequested()
    {
        if (!selectionSession.TrySelectPreviousOffer()) return;

        RenderMenu();
    }

    private void HandleNextOfferRequested()
    {
        if (!selectionSession.TrySelectNextOffer()) return;

        RenderMenu();
    }

    private IReadOnlyList<AbilityOffer> BuildOffers()
    {
        if (!abilityCatalog)
            return Array.Empty<AbilityOffer>();

        if (!abilityController)
            return Array.Empty<AbilityOffer>();

        var ownedAbilityIds = new List<string>(abilityController.AbilitySlotCount);
        var hasEmptyAbilitySlot = false;

        for (var i = 0; i < abilityController.AbilitySlotCount; i++)
        {
            var ability = abilityController.GetAbilityInSlot(i);
            if (ability == null)
            {
                hasEmptyAbilitySlot = true;
                continue;
            }

            if (!string.IsNullOrEmpty(ability.Id))
                ownedAbilityIds.Add(ability.Id);
        }

        return offerService.BuildOffers(new AbilityOfferRequest(abilityCatalog, ownedAbilityIds, hasEmptyAbilitySlot));
    }

    private bool CanOpenMenuInternal()
    {
        if (!isEnabled) return false;
        if (progression.AvailableAbilityPoints <= 0) return false;
        if (!abilitySelectionMenu) return false;
        if (!inputReader) return false;

        if (!abilityController) return false;

        return isMenuInteractionAllowed();
    }

    private bool CanKeepMenuOpen()
    {
        if (!isEnabled) return false;
        if (!abilitySelectionMenu) return false;
        if (!inputReader) return false;

        if (!abilityController) return false;

        return isMenuInteractionAllowed();
    }

    private void RenderMenu()
    {
        if (!abilitySelectionMenu) return;

        if (selectionSession.State != ParticipantAbilitySelectionSession.SessionState.SelectingOffer)
        {
            abilitySelectionMenu.Close();
            return;
        }

        abilitySelectionMenu.Show(selectionSession.Offers, selectionSession.SelectedOfferIndex);
    }
}
