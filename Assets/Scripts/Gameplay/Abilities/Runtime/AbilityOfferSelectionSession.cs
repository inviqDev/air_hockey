using System;
using System.Collections.Generic;

public sealed class AbilityOfferSelectionSession
{
    public enum SessionState
    {
        Closed,
        SelectingOffer
    }

    private IReadOnlyList<AbilityOffer> offers = Array.Empty<AbilityOffer>();
    private int selectedOfferIndex = -1;

    public SessionState State { get; private set; }
    public IReadOnlyList<AbilityOffer> Offers => offers;
    public int SelectedOfferIndex => selectedOfferIndex;

    public bool TryOpen(IReadOnlyList<AbilityOffer> nextOffers)
    {
        if (nextOffers == null || nextOffers.Count == 0)
        {
            Close();
            return false;
        }

        offers = nextOffers;
        selectedOfferIndex = 0;
        State = SessionState.SelectingOffer;
        return true;
    }

    public bool TrySelectOffer(int index)
    {
        if (State != SessionState.SelectingOffer) return false;
        if (index < 0 || index >= offers.Count) return false;
        if (selectedOfferIndex == index) return true;

        selectedOfferIndex = index;
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
        State = SessionState.Closed;
        offers = System.Array.Empty<AbilityOffer>();
        selectedOfferIndex = -1;
    }

    private bool TrySelectRelative(int offset)
    {
        if (State != SessionState.SelectingOffer) return false;

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
}
