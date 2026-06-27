using System.Collections.Generic;
using UnityEngine;

public sealed class AbilitySelectionViewContainer : MonoBehaviour
{
    [SerializeField] private AbilityOffersView abilityOffersView;
    [SerializeField] private RectTransform slotSelectionRoot;

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

        if (slotSelectionRoot)
            slotSelectionRoot.gameObject.SetActive(false);

        abilityOffersView.Show(offers, selectedOfferIndex);
    }

    public void ShowSlotSelection()
    {
        if (!slotSelectionRoot)
        {
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires a slot-selection root reference.", this);
            return;
        }

        gameObject.SetActive(true);

        if (abilityOffersView)
            abilityOffersView.Close();

        slotSelectionRoot.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (abilityOffersView)
            abilityOffersView.Close();

        if (slotSelectionRoot)
            slotSelectionRoot.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    private void ValidateReferences()
    {
        if (!abilityOffersView)
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires an {nameof(AbilityOffersView)} reference.", this);

        if (!slotSelectionRoot)
            Debug.LogError($"{nameof(AbilitySelectionViewContainer)} on {name} requires a slot-selection root reference.", this);
    }
}
