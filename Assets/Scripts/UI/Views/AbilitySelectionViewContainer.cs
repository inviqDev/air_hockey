using System.Collections.Generic;
using UnityEngine;

public sealed class AbilitySelectionViewContainer : MonoBehaviour
{
    [SerializeField] private AbilityOffersView abilityOffersView;
    [SerializeField] private AbilitySlotSelectionView slotSelectionView;

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void ShowOffers(IReadOnlyList<AbilityOffer> offers, int selectedOfferIndex)
    {
        if (!abilityOffersView)
        {
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires an {nameof(AbilityOffersView)} reference.", this);
            return;
        }

        gameObject.SetActive(true);

        if (slotSelectionView)
            slotSelectionView.Close();

        abilityOffersView.Show(offers, selectedOfferIndex);
    }

    public void ShowSlotSelection(IReadOnlyList<AbilitySlotData> slots, int selectedSlotIndex)
    {
        if (!slotSelectionView)
        {
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires an {nameof(AbilitySlotSelectionView)} reference.", this);
            return;
        }

        gameObject.SetActive(true);

        if (abilityOffersView)
            abilityOffersView.Close();

        slotSelectionView.Show(slots, selectedSlotIndex);
    }

    public void Close()
    {
        if (abilityOffersView)
            abilityOffersView.Close();

        if (slotSelectionView)
            slotSelectionView.Close();

        gameObject.SetActive(false);
    }

    private void ValidateReferences()
    {
        if (!abilityOffersView)
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires an {nameof(AbilityOffersView)} reference.", this);

        if (!slotSelectionView)
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires an {nameof(AbilitySlotSelectionView)} reference.", this);
    }
}
