using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AbilityOffersView  : MonoBehaviour
{
    [SerializeField] private Color highlightedColor;

    [SerializeField] private Transform buttonsRoot;
    [SerializeField] private Button[] offerButtons;
    
    [SerializeField] private TMP_Text descriptionLabel;

    public event Action<int> SelectedOfferClicked;

    public bool IsOpen => gameObject.activeSelf;

    private readonly Dictionary<Button, int> buttonIndexes = new();
    private readonly Dictionary<Button, UnityEngine.Events.UnityAction> buttonClickHandlers = new();
    private readonly Dictionary<Button, Color> defaultButtonColors = new();

    private void Awake()
    {
        ValidateReferences();
        RebuildButtonIndexMap();
    }

    private void OnEnable()
    {
        SubscribeToButtons();
    }

    private void OnDisable()
    {
        UnsubscribeFromButtons();
    }

    private void OnValidate()
    {
        ValidateReferences();
        RebuildButtonIndexMap();
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
        for (var i = 0; i < offerButtons.Length; i++)
        {
            var hasOffer = offers != null && i < offers.Count;
            var button = offerButtons[i];
            if (!button) continue;

            button.gameObject.SetActive(hasOffer);
            button.interactable = hasOffer;
            ApplyButtonSelectionVisual(button, hasOffer && i == selectedOfferIndex);
        }

        if (descriptionLabel && offers != null)
        {
            descriptionLabel.text = IsValidOfferIndex(offers, selectedOfferIndex)
                ? GetOfferDescriptionText(offers[selectedOfferIndex])
                : "No offers available.";
        }
    }

    private void HandleOfferButtonClicked(Button clickedButton)
    {
        if (!buttonIndexes.TryGetValue(clickedButton, out var buttonIndex)) return;
        SelectedOfferClicked?.Invoke(buttonIndex);
    }

    private void ValidateReferences()
    {
        if (!descriptionLabel)
            Debug.LogError($"{nameof(AbilityOffersView)} on {name} requires a description label reference.", this);

        if (!buttonsRoot)
            Debug.LogError($"{nameof(AbilityOffersView)} on {name} requires a buttons root reference.", this);

        if (offerButtons == null || offerButtons.Length == 0)
        {
            Debug.LogError($"{nameof(AbilityOffersView)} on {name} requires offer button references.", this);
            return;
        }

        for (var i = 0; i < offerButtons.Length; i++)
        {
            if (offerButtons[i]) continue;

            Debug.LogError(
                $"{nameof(AbilityOffersView)} on {name} requires an offer button reference for index {i}.", this);
        }
    }

    private void RebuildButtonIndexMap()
    {
        buttonIndexes.Clear();

        if (offerButtons == null) return;

        for (var i = 0; i < offerButtons.Length; i++)
        {
            var button = offerButtons[i];
            if (!button) continue;

            buttonIndexes[button] = i;
            CacheDefaultButtonColor(button);
        }
    }

    private void SubscribeToButtons()
    {
        if (offerButtons == null) return;

        foreach (var button in offerButtons)
        {
            if (!button) continue;
            if (buttonClickHandlers.ContainsKey(button)) continue;

            UnityEngine.Events.UnityAction handler = () => HandleOfferButtonClicked(button);
            buttonClickHandlers[button] = handler;
            button.onClick.AddListener(handler);
        }
    }

    private void UnsubscribeFromButtons()
    {
        if (offerButtons == null) return;

        foreach (var button in offerButtons)
        {
            if (!button) continue;
            if (!buttonClickHandlers.TryGetValue(button, out var handler)) continue;

            button.onClick.RemoveListener(handler);
        }

        buttonClickHandlers.Clear();
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

    private void ApplyButtonSelectionVisual(Button button, bool isSelected)
    {
        if (!button || !button.targetGraphic) return;

        CacheDefaultButtonColor(button);
        
        if (!defaultButtonColors.TryGetValue(button, out var defaultColor)) return;

        button.targetGraphic.color = isSelected
            ? highlightedColor
            : defaultColor;
    }

    private void CacheDefaultButtonColor(Button button)
    {
        if (!button || !button.targetGraphic) return;
        if (defaultButtonColors.ContainsKey(button)) return;

        defaultButtonColors[button] = button.targetGraphic.color;
    }
}
