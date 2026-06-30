using System.Collections.Generic;
using UnityEngine;

public sealed class AbilitySelectionViewContainer : MonoBehaviour
{
    [SerializeField] private AbilityOfferSelectionView offerSelectionView;
    [SerializeField] private AbilitySlotSelectionView slotSelectionView;

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void ShowOffers(IReadOnlyList<AbilityOffer> offers, int selectedOfferIndex)
    {
        if (!offerSelectionView)
        {
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires an {nameof(AbilityOfferSelectionView)} reference.", this);
            return;
        }

        gameObject.SetActive(true);

        if (slotSelectionView)
            slotSelectionView.Close();

        offerSelectionView.Show(offers, selectedOfferIndex);
    }

    public void ShowSlotSelection(IReadOnlyList<AbilitySlotData> slots, int selectedSlotIndex)
    {
        if (!slotSelectionView)
        {
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires an {nameof(AbilitySlotSelectionView)} reference.", this);
            return;
        }

        gameObject.SetActive(true);

        if (offerSelectionView)
            offerSelectionView.Close();

        slotSelectionView.Show(slots, selectedSlotIndex);
    }

    public void Close()
    {
        if (offerSelectionView)
            offerSelectionView.Close();

        if (slotSelectionView)
            slotSelectionView.Close();

        gameObject.SetActive(false);
    }

    private void ValidateReferences()
    {
        if (!offerSelectionView)
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires an {nameof(AbilityOfferSelectionView)} reference.", this);

        if (!slotSelectionView)
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires an {nameof(AbilitySlotSelectionView)} reference.", this);
    }
}
