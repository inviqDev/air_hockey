using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class AbilityOffersView : MonoBehaviour
{
    [SerializeField] private AbilitySelectionItemsContainer offerItemsContainer;
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
        var itemCount = offerItemsContainer ? offerItemsContainer.ItemCount : 0;

        for (var i = 0; i < itemCount; i++)
        {
            var itemView = offerItemsContainer.GetItemView(i);
            if (!itemView) continue;

            var hasOffer = offers != null && i < offers.Count;
            itemView.SetVisible(hasOffer);

            if (hasOffer)
                itemView.SetIcon(offers[i].Config ? offers[i].Config.Icon : null);
            else
                itemView.SetIcon(null);

            itemView.SetSelected(hasOffer && i == selectedOfferIndex);
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

        if (!offerItemsContainer)
            Debug.LogError($"{nameof(AbilityOffersView)} on {name} requires an {nameof(AbilitySelectionItemsContainer)} reference.", this);
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
