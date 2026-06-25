using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AbilityOffersView : MonoBehaviour
{
    [SerializeField] private Image[] offerIcons;
    [SerializeField] private TMP_Text descriptionLabel;

    public bool IsOpen => gameObject.activeSelf;

    private Outline[] offerSelectionOutlines = System.Array.Empty<Outline>();

    private void Awake()
    {
        ValidateReferences();
        RebuildSelectionVisualCache();
    }

    private void OnValidate()
    {
        ValidateReferences();
        RebuildSelectionVisualCache();
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
        for (var i = 0; i < offerIcons.Length; i++)
        {
            var hasOffer = offers != null && i < offers.Count;
            var offerIcon = offerIcons[i];
            if (!offerIcon) continue;

            offerIcon.gameObject.SetActive(hasOffer);

            if (hasOffer)
                SetOfferIcon(offerIcon, offers[i]);

            ApplySelectionVisual(i, hasOffer && i == selectedOfferIndex);
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

        if (offerIcons == null || offerIcons.Length == 0)
        {
            Debug.LogError($"{nameof(AbilityOffersView)} on {name} requires offer icon references.", this);
            return;
        }

        for (var i = 0; i < offerIcons.Length; i++)
        {
            var offerIcon = offerIcons[i];
            if (!offerIcon)
            {
                Debug.LogError(
                    $"{nameof(AbilityOffersView)} on {name} requires an offer icon reference for index {i}.", this);
                continue;
            }

            if (!offerIcon.GetComponent<Outline>())
            {
                Debug.LogError(
                    $"{nameof(AbilityOffersView)} on {name} requires a preconfigured {nameof(Outline)} on offer icon index {i}.",
                    this);
            }
        }
    }

    private void RebuildSelectionVisualCache()
    {
        if (offerIcons == null)
        {
            offerSelectionOutlines = System.Array.Empty<Outline>();
            return;
        }

        offerSelectionOutlines = new Outline[offerIcons.Length];

        for (var i = 0; i < offerIcons.Length; i++)
        {
            var offerIcon = offerIcons[i];
            if (!offerIcon) continue;

            var selectionOutline = offerIcon.GetComponent<Outline>();
            offerSelectionOutlines[i] = selectionOutline;

            if (selectionOutline)
                selectionOutline.enabled = false;
        }
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

    private void ApplySelectionVisual(int offerIndex, bool isSelected)
    {
        if (offerSelectionOutlines == null) return;
        if (offerIndex < 0 || offerIndex >= offerSelectionOutlines.Length) return;

        var selectionOutline = offerSelectionOutlines[offerIndex];
        if (!selectionOutline) return;

        selectionOutline.enabled = isSelected;
    }

    private static void SetOfferIcon(Image offerIcon, AbilityOffer offer)
    {
        if (!offerIcon) return;

        var icon = offer.Config ? offer.Config.Icon : null;
        offerIcon.sprite = icon;
        offerIcon.enabled = icon;
    }
}
