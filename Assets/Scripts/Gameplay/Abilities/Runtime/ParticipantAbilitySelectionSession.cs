using System.Collections.Generic;

public sealed class ParticipantAbilitySelectionSession
{
    public enum SessionState
    {
        Closed,
        SelectingOffer
    }

    private IReadOnlyList<AbilityOffer> offers = System.Array.Empty<AbilityOffer>();
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

    public void Close()
    {
        State = SessionState.Closed;
        offers = System.Array.Empty<AbilityOffer>();
        selectedOfferIndex = -1;
    }
}
