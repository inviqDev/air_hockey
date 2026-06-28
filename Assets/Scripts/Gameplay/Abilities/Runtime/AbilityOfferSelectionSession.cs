using System;
using System.Collections.Generic;
using UnityEngine;

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
        selectedSlotIndex = -1;
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

    public bool TrySelectPreviousSlot(IReadOnlyList<AbilitySlotData> slots)
    {
        return TrySelectRelativeEmptySlot(slots, -1);
    }

    public bool TrySelectNextSlot(IReadOnlyList<AbilitySlotData> slots)
    {
        return TrySelectRelativeEmptySlot(slots, 1);
    }

    public void Close()
    {
        State = AbilityOfferSelectionState.Closed;
        offers = Array.Empty<AbilityOffer>();
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

    private bool TrySelectRelativeEmptySlot(IReadOnlyList<AbilitySlotData> slots, int offset)
    {
        if (State != AbilityOfferSelectionState.SelectingSlot) return false;
        if (slots == null || slots.Count == 0) return false;
        if (!HasValidSelectedEmptySlot(slots)) return false;

        for (var i = 1; i < slots.Count; i++)
        {
            var directionOffset = i * offset;
            var nextIndex = selectedSlotIndex + directionOffset;
            var wrappedIndex = WrapIndex(nextIndex, slots.Count);
            
            if (slots[wrappedIndex].HasAbility) continue;

            selectedSlotIndex = wrappedIndex;
            return true;
        }

        return false;
    }
    
    private bool HasValidSelectedEmptySlot(IReadOnlyList<AbilitySlotData> slots)
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= slots.Count)
        {
            Debug.LogError(
                $"{nameof(AbilityOfferSelectionSession)} has invalid selected slot index " +
                $"{selectedSlotIndex} while selecting a slot.");

            return false;
        }

        if (slots[selectedSlotIndex].HasAbility)
        {
            Debug.LogError(
                $"{nameof(AbilityOfferSelectionSession)} selected occupied slot " +
                $"{selectedSlotIndex} while selecting a slot.");

            return false;
        }

        return true;
    }
    
    private static int WrapIndex(int index, int count)
    {
        var wrappedIndex = index % count;
        return wrappedIndex < 0 ? wrappedIndex + count : wrappedIndex;
    }
}
