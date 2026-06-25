using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class AbilityOffersView : MonoBehaviour
{
    [SerializeField] private AbilityOfferSlotsContainer offerSlotsContainer;
    [SerializeField] private TMP_Text descriptionLabel;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void Show(IReadOnlyList<AbilityOffer> offers, int selectedOfferIndex)
    {
        gameObject.SetActive(true);
        Render(offers, selectedOfferIndex);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Render(IReadOnlyList<AbilityOffer> offers, int selectedOfferIndex)
    {
        var slotCount = offerSlotsContainer ? offerSlotsContainer.SlotCount : 0;

        for (var i = 0; i < slotCount; i++)
        {
            var slotView = offerSlotsContainer.GetSlotView(i);
            if (!slotView) continue;

            var hasOffer = offers != null && i < offers.Count;
            slotView.SetVisible(hasOffer);

            if (hasOffer)
                slotView.SetOfferIcon(offers[i].Config ? offers[i].Config.Icon : null);
            else
                slotView.SetOfferIcon(null);

            slotView.SetSelected(hasOffer && i == selectedOfferIndex);
        }

        if (descriptionLabel && offers != null)
        {
            descriptionLabel.text = IsValidOfferIndex(offers, selectedOfferIndex)
                ? GetOfferDescriptionText(offers[selectedOfferIndex])
                : "No offers available.";
        }
    }

    private void ValidateReferences()
    {
        if (!descriptionLabel)
            Debug.LogError($"{nameof(AbilityOffersView)} on {name} requires a description label reference.", this);

        if (!offerSlotsContainer)
            Debug.LogError($"{nameof(AbilityOffersView)} on {name} requires an {nameof(AbilityOfferSlotsContainer)} reference.", this);
    }

    private static string GetOfferDescriptionText(AbilityOffer offer)
    {
        var abilityName = offer.Config ? offer.Config.DisplayName : "Missing Ability";
        var description = offer.Config ? offer.Config.Description : string.Empty;
        var kindLabel = offer.Kind == AbilityOfferKind.Upgrade ? "Upgrade" : "New Ability";

        return string.IsNullOrWhiteSpace(description) 
            ? $"{kindLabel}\n{abilityName}" 
            : $"{kindLabel}\n{abilityName}\n\n{description}";
    }

    private static bool IsValidOfferIndex(IReadOnlyList<AbilityOffer> offers, int index)
    {
        return offers != null && index >= 0 && index < offers.Count;
    }

}
