using System;
using System.Collections.Generic;
using UnityEngine;

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
    private Func<bool> canOpenMenuPredicate = AllowMenuOpen;

    public event Action<bool> MenuOpenStateChanged;

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

    public bool CanOpenMenu => CanInteractWithMenu(requireAvailablePoints: true) && canOpenMenuPredicate();
    public bool IsMenuOpen => offerSelectionSession.State != AbilityOfferSelectionState.Closed;

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

    public void BindAbilityController(PlayerAbilityController controller)
    {
        if (abilityController == controller) return;
        if (offerSelectionSession.State != AbilityOfferSelectionState.Closed)
            CloseMenu();

        abilityController = controller;
        
        var nextInputReader = abilityController ? abilityController.InputReader : null;
        BindInputReader(nextInputReader);
    }

    public void SetCanOpenMenuPredicate(Func<bool> predicate)
    {
        canOpenMenuPredicate = predicate ?? AllowMenuOpen;
    }

    public void CloseMenu()
    {
        var wasMenuOpen = IsMenuOpen;
        offerSelectionSession.Close();
        selectionViewContainer.Close();

        if (wasMenuOpen)
            MenuOpenStateChanged?.Invoke(false);
    }

    public void Tick()
    {
        if (offerSelectionSession.State == AbilityOfferSelectionState.Closed) return;
        if (!CanInteractWithMenu(requireAvailablePoints: false))
            CloseMenu();
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

    private void SubscribeToHud()
    {
        participantHud.PlusAbilityButtonClicked += HandleMenuToggleRequested;
    }

    private void UnsubscribeFromHud()
    {
        participantHud.PlusAbilityButtonClicked -= HandleMenuToggleRequested;
    }

    private void SubscribeToInputReader()
    {
        if (!inputReader) return;

        inputReader.AbilitySelectionMenuPressed += HandleMenuToggleRequested;
        inputReader.AbilitySelectionPreviousPressed += HandlePreviousSelectionRequested;
        inputReader.AbilitySelectionNextPressed += HandleNextSelectionRequested;
        inputReader.AbilitySelectionConfirmPressed += HandleConfirmRequested;
        inputReader.AbilitySelectionBackPressed += HandleBackRequested;
    }

    private void UnsubscribeFromInputReader()
    {
        if (!inputReader) return;

        inputReader.AbilitySelectionMenuPressed -= HandleMenuToggleRequested;
        inputReader.AbilitySelectionPreviousPressed -= HandlePreviousSelectionRequested;
        inputReader.AbilitySelectionNextPressed -= HandleNextSelectionRequested;
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

        if (!CanOpenMenu) return;

        OpenMenu();
    }

    private void HandlePreviousSelectionRequested()
    {
        if (!TrySelectPrevious()) return;

        RenderMenu();
    }

    private void HandleNextSelectionRequested()
    {
        if (!TrySelectNext()) return;

        RenderMenu();
    }

    private void HandleConfirmRequested()
    {
        switch (offerSelectionSession.State)
        {
            case AbilityOfferSelectionState.SelectingOffer:
                EnterSlotSelection();
                return;

            case AbilityOfferSelectionState.SelectingSlot:
                ConfirmSelectedSlotAssignment();
                return;
        }
    }

    private void HandleBackRequested()
    {
        if (!offerSelectionSession.TryReturnToOfferSelection()) return;

        RenderMenu();
    }

    private void OpenMenu()
    {
        var offers = BuildOffers();
        if (!offerSelectionSession.TryOpen(offers)) return;

        RenderMenu();
        MenuOpenStateChanged?.Invoke(true);
    }

    private void ReopenMenuWithRefreshedOffers()
    {
        var refreshedOffers = BuildOffers();
        if (!offerSelectionSession.TryOpen(refreshedOffers))
        {
            CloseMenu();
            return;
        }

        RenderMenu();
    }

    private void EnterSlotSelection()
    {
        if (!TryFindFirstEmptySlotIndex(out var firstEmptySlotIndex)) return;
        if (!offerSelectionSession.TryEnterSlotSelection(firstEmptySlotIndex)) return;

        RenderMenu();
    }

    private void ConfirmSelectedSlotAssignment()
    {
        if (!TryGetSelectedNewAbilityOffer(out var selectedOffer)) return;
        if (pointsProgression.AvailableAbilityPoints <= 0)
        {
            Debug.LogError(
                $"{nameof(AbilityOfferSelectionFlow)} cannot confirm slot selection " +
                $"because the participant has no available ability points.");

            return;
        }

        var selectedSlotIndex = offerSelectionSession.SelectedSlotIndex;
        if (!abilityController.TryAddAbilityToEmptySlot(selectedOffer.Config, selectedSlotIndex)) return;
        if (!pointsProgression.TrySpendAvailableAbilityPoint())
        {
            Debug.LogError(
                $"{nameof(AbilityOfferSelectionFlow)} assigned a new ability to slot {selectedSlotIndex} " +
                $"but failed to spend an available ability point. This violates the selection transaction invariant.");

            return;
        }

        if (pointsProgression.AvailableAbilityPoints <= 0)
        {
            CloseMenu();
            return;
        }

        ReopenMenuWithRefreshedOffers();
    }

    private bool TryGetSelectedNewAbilityOffer(out AbilityOffer selectedOffer)
    {
        selectedOffer = default;

        var offers = offerSelectionSession.Offers;
        var selectedOfferIndex = offerSelectionSession.SelectedOfferIndex;

        if (selectedOfferIndex < 0 || selectedOfferIndex >= offers.Count)
        {
            Debug.LogError(
                $"{nameof(AbilityOfferSelectionFlow)} cannot confirm slot selection " +
                $"because selected offer index {selectedOfferIndex} is invalid.");

            return false;
        }

        selectedOffer = offers[selectedOfferIndex];
        if (selectedOffer.Kind == AbilityOfferKind.NewAbility) return true;

        Debug.LogError(
            $"{nameof(AbilityOfferSelectionFlow)} cannot confirm slot selection " +
            $"for offer kind {selectedOffer.Kind}.");

        selectedOffer = default;
        return false;
    }

    private bool TrySelectPrevious()
    {
        switch (offerSelectionSession.State)
        {
            case AbilityOfferSelectionState.SelectingOffer:
                return offerSelectionSession.TrySelectPreviousOffer();

            case AbilityOfferSelectionState.SelectingSlot:
                var slotSnapshot = BuildSlotDataSnapshot();
                return offerSelectionSession.TrySelectPreviousSlot(slotSnapshot);

            default:
                return false;
        }
    }

    private bool TrySelectNext()
    {
        switch (offerSelectionSession.State)
        {
            case AbilityOfferSelectionState.SelectingOffer:
                return offerSelectionSession.TrySelectNextOffer();

            case AbilityOfferSelectionState.SelectingSlot:
                var slotSnapshot = BuildSlotDataSnapshot();
                return offerSelectionSession.TrySelectNextSlot(slotSnapshot);
            
            default:
                return false;
        }
    }

    private bool CanInteractWithMenu(bool requireAvailablePoints)
    {
        if (!isEnabled || !inputReader || !abilityController) return false;
        if (requireAvailablePoints && pointsProgression.AvailableAbilityPoints <= 0) return false;

        return isMenuInteractionAllowed();
    }

    private static bool AllowMenuOpen()
    {
        return true;
    }

    private IReadOnlyList<AbilityOffer> BuildOffers()
    {
        var slotCount = abilityController.AbilitySlotCount;
        var ownedAbilityIds = new List<string>(slotCount);
        var hasEmptyAbilitySlot = false;

        for (var i = 0; i < slotCount; i++)
        {
            var ability = abilityController.GetAbilityInSlot(i);

            if (ability == null)
            {
                hasEmptyAbilitySlot = true;
                continue;
            }

            if (!string.IsNullOrEmpty(ability.Id))
            {
                ownedAbilityIds.Add(ability.Id);
            }
        }

        var request = new AbilityOfferRequest(abilityCatalog, ownedAbilityIds, hasEmptyAbilitySlot);
        return offerService.BuildOffers(request);
    }

    private AbilitySlotData[] BuildSlotDataSnapshot()
    {
        var slotCount = abilityController.AbilitySlotCount;
        var slotSnapshot = new AbilitySlotData[slotCount];

        for (var i = 0; i < slotCount; i++)
        {
            slotSnapshot[i] = abilityController.GetAbilitySlotData(i);
        }

        return slotSnapshot;
    }

    private bool TryFindFirstEmptySlotIndex(out int slotIndex)
    {
        slotIndex = -1;

        for (var i = 0; i < abilityController.AbilitySlotCount; i++)
        {
            if (abilityController.GetAbilityInSlot(i) != null) continue;

            slotIndex = i;
            return true;
        }

        return false;
    }

    private void RenderMenu()
    {
        switch (offerSelectionSession.State)
        {
            case AbilityOfferSelectionState.Closed:
                selectionViewContainer.Close();
                return;

            case AbilityOfferSelectionState.SelectingOffer:
                ShowOfferSelection();
                return;

            case AbilityOfferSelectionState.SelectingSlot:
                ShowSlotSelection();
                return;
        }
    }

    private void ShowSlotSelection()
    {
        var playerSlotsSnapshot = BuildSlotDataSnapshot();
        var selectedSlotIndex = offerSelectionSession.SelectedSlotIndex;
        selectionViewContainer.ShowSlotSelection(playerSlotsSnapshot, selectedSlotIndex);
    }

    private void ShowOfferSelection()
    {
        var offers = offerSelectionSession.Offers;
        var selectedOfferIndex = offerSelectionSession.SelectedOfferIndex;
        selectionViewContainer.ShowOffers(offers, selectedOfferIndex);
    }
}
