using System;
using System.Collections.Generic;

public sealed class AbilityOfferSelectionFlow
{
    private readonly ParticipantHudView participantHud;
    private readonly AbilityPointsProgression pointsProgression;
    private readonly AbilitySelectionViewContainer selectionViewContainer;
    private readonly AbilityOfferService offerService;
    private readonly AbilityCatalog abilityCatalog;
    private readonly Func<bool> isMenuInteractionAllowed;
    private readonly AbilityOfferSelectionSession offerSelectionSession = new();

    private PlayerAbilityController abilityController;
    private PlayerInputReader inputReader;
    private bool isEnabled;

    public AbilityOfferSelectionFlow(
        ParticipantHudView participantHud,
        AbilitySelectionViewContainer selectionViewContainer,
        AbilityPointsProgression pointsProgression,
        AbilityCatalog abilityCatalog,
        AbilityOfferService offerService,
        Func<bool> isMenuInteractionAllowed)
    {
        this.participantHud = participantHud;
        this.selectionViewContainer = selectionViewContainer;
        this.pointsProgression = pointsProgression;
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
        SubscribeToInputReader();
    }

    public void Disable()
    {
        if (!isEnabled) return;

        CloseMenu();
        UnsubscribeFromInputReader();
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
        offerSelectionSession.Close();

        if (!selectionViewContainer) return;
        selectionViewContainer.Close();
    }

    public void Tick()
    {
        if (offerSelectionSession.State == AbilityOfferSelectionState.Closed) return;
        if (CanInteractWithMenu(requireAvailablePoints: false)) return;

        CloseMenu();
    }

    private void SubscribeToHud()
    {
        if (!participantHud) return;
        participantHud.PlusAbilityButtonClicked += HandleMenuToggleRequested;
    }

    private void UnsubscribeFromHud()
    {
        if (!participantHud) return;
        participantHud.PlusAbilityButtonClicked -= HandleMenuToggleRequested;
    }

    private void SubscribeToInputReader()
    {
        if (!inputReader) return;

        inputReader.AbilitySelectionMenuPressed += HandleMenuToggleRequested;
        inputReader.AbilitySelectionPreviousPressed += HandlePreviousOfferRequested;
        inputReader.AbilitySelectionNextPressed += HandleNextOfferRequested;
        inputReader.AbilitySelectionConfirmPressed += HandleConfirmRequested;
        inputReader.AbilitySelectionBackPressed += HandleBackRequested;
    }

    private void UnsubscribeFromInputReader()
    {
        if (!inputReader) return;

        inputReader.AbilitySelectionMenuPressed -= HandleMenuToggleRequested;
        inputReader.AbilitySelectionPreviousPressed -= HandlePreviousOfferRequested;
        inputReader.AbilitySelectionNextPressed -= HandleNextOfferRequested;
        inputReader.AbilitySelectionConfirmPressed -= HandleConfirmRequested;
        inputReader.AbilitySelectionBackPressed -= HandleBackRequested;
    }

    private void HandleMenuToggleRequested()
    {
        if (offerSelectionSession.State != AbilityOfferSelectionState.Closed)
        {
            CloseMenu();
            return;
        }

        if (!CanInteractWithMenu(requireAvailablePoints: true)) return;

        var offers = BuildOffers();
        if (!offerSelectionSession.TryOpen(offers)) return;

        RenderMenu();
    }

    private void HandlePreviousOfferRequested()
    {
        if (!offerSelectionSession.TrySelectPreviousOffer()) return;

        RenderMenu();
    }

    private void HandleNextOfferRequested()
    {
        if (!offerSelectionSession.TrySelectNextOffer()) return;

        RenderMenu();
    }

    private void HandleConfirmRequested()
    {
        switch (offerSelectionSession.State)
        {
            case AbilityOfferSelectionState.SelectingOffer:
                var firstEmptySlotIndex = FindFirstEmptySlotIndex();
                if (!offerSelectionSession.TryEnterSlotSelection(firstEmptySlotIndex)) return;
                RenderMenu();
                break;
            case AbilityOfferSelectionState.SelectingSlot:
                break;
        }
    }

    private void HandleBackRequested()
    {
        if (!offerSelectionSession.TryReturnToOfferSelection()) return;

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
        return CanInteractWithMenu(requireAvailablePoints: true);
    }

    private bool CanInteractWithMenu(bool requireAvailablePoints)
    {
        if (!isEnabled) return false;
        if (!selectionViewContainer) return false;
        if (!inputReader) return false;
        if (!abilityController) return false;
        if (requireAvailablePoints && pointsProgression.AvailableAbilityPoints <= 0) return false;

        return isMenuInteractionAllowed();
    }

    private void RenderMenu()
    {
        if (!selectionViewContainer) return;

        switch (offerSelectionSession.State)
        {
            case AbilityOfferSelectionState.Closed:
                selectionViewContainer.Close();
                return;
            case AbilityOfferSelectionState.SelectingOffer:
                selectionViewContainer.ShowOffers(offerSelectionSession.Offers, offerSelectionSession.SelectedOfferIndex);
                return;
            case AbilityOfferSelectionState.SelectingSlot:
                selectionViewContainer.ShowSlotSelection(BuildSlotDataSnapshot(), offerSelectionSession.SelectedSlotIndex);
                return;
        }
    }

    private AbilitySlotData[] BuildSlotDataSnapshot()
    {
        if (!abilityController)
            return Array.Empty<AbilitySlotData>();

        var slotCount = abilityController.AbilitySlotCount;
        var slots = new AbilitySlotData[slotCount];

        for (var i = 0; i < slotCount; i++)
            slots[i] = abilityController.GetAbilitySlotData(i);

        return slots;
    }

    private int FindFirstEmptySlotIndex()
    {
        if (!abilityController)
            return -1;

        for (var i = 0; i < abilityController.AbilitySlotCount; i++)
        {
            if (abilityController.GetAbilityInSlot(i) == null)
                return i;
        }

        return -1;
    }
}
