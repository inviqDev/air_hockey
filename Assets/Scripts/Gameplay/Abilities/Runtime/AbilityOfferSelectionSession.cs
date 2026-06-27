using System;
using System.Collections.Generic;

public enum AbilityOfferSelectionState
{
    Closed,
    SelectingOffer,
    SelectingSlot
}

public sealed class AbilityOfferSelectionSession
{
    private IReadOnlyList<AbilityOffer> offers = Array.Empty<AbilityOffer>();
    private int selectedOfferIndex = -1;
    private int selectedSlotIndex = -1;

    public AbilityOfferSelectionState State { get; private set; }
    public IReadOnlyList<AbilityOffer> Offers => offers;
    public int SelectedOfferIndex => selectedOfferIndex;
    public int SelectedSlotIndex => selectedSlotIndex;

    public bool TryOpen(IReadOnlyList<AbilityOffer> nextOffers)
    {
        if (nextOffers == null || nextOffers.Count == 0)
        {
            Close();
            return false;
        }

        offers = nextOffers;
        selectedOfferIndex = 0;
        selectedSlotIndex = -1;
        State = AbilityOfferSelectionState.SelectingOffer;
        return true;
    }

    public bool TrySelectOffer(int index)
    {
        if (State != AbilityOfferSelectionState.SelectingOffer) return false;
        if (index < 0 || index >= offers.Count) return false;
        if (selectedOfferIndex == index) return false;

        selectedOfferIndex = index;
        return true;
    }

    public bool TryEnterSlotSelection(int initialSelectedSlotIndex)
    {
        if (State != AbilityOfferSelectionState.SelectingOffer) return false;
        if (!IsValidOfferIndex(selectedOfferIndex)) return false;
        if (offers[selectedOfferIndex].Kind != AbilityOfferKind.NewAbility) return false;
        if (initialSelectedSlotIndex < 0) return false;

        State = AbilityOfferSelectionState.SelectingSlot;
        selectedSlotIndex = initialSelectedSlotIndex;

        return true;
    }

    public bool TryReturnToOfferSelection()
    {
        if (State != AbilityOfferSelectionState.SelectingSlot) return false;
        if (!IsValidOfferIndex(selectedOfferIndex)) return false;

        State = AbilityOfferSelectionState.SelectingOffer;
        return true;
    }

    public bool TrySelectPreviousOffer()
    {
        return TrySelectRelative(-1);
    }

    public bool TrySelectNextOffer()
    {
        return TrySelectRelative(1);
    }

    public void Close()
    {
        State = AbilityOfferSelectionState.Closed;
        offers = System.Array.Empty<AbilityOffer>();
        selectedOfferIndex = -1;
        selectedSlotIndex = -1;
    }

    private bool TrySelectRelative(int offset)
    {
        if (State != AbilityOfferSelectionState.SelectingOffer) return false;

        var offerCount = offers.Count;
        if (offerCount <= 1)
        {
            if (offerCount == 1 && selectedOfferIndex < 0)
                selectedOfferIndex = 0;

            return false;
        }

        var nextIndex = (selectedOfferIndex + offset) % offerCount;
        if (nextIndex < 0)
            nextIndex += offerCount;

        if (nextIndex == selectedOfferIndex) return false;

        selectedOfferIndex = nextIndex;
        return true;
    }

    private bool IsValidOfferIndex(int index)
    {
        return index >= 0 && index < offers.Count;
    }
}
