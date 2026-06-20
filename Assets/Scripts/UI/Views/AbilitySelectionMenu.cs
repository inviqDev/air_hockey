using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AbilitySelectionMenu : MonoBehaviour
{
    [Header("Optional References")]
    [SerializeField] private TMP_Text selectionLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private Transform selectionButtonsRoot;
    [SerializeField] private Button[] offerButtons;
    [SerializeField] private TMP_Text[] offerButtonLabels;

    public event Action<int> OfferClicked;

    public bool IsOpen => gameObject.activeSelf;

    private readonly Dictionary<Button, int> buttonIndexes = new();
    private readonly Dictionary<Button, UnityEngine.Events.UnityAction> buttonClickHandlers = new();

    private void Awake()
    {
        CacheReferences();
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
        CacheReferences();
        RebuildButtonIndexMap();
    }

    public void Show(IReadOnlyList<AbilityOffer> offers, int selectedOfferIndex)
    {
        CacheReferences();
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

            var text = i < offerButtonLabels.Length
                ? offerButtonLabels[i]
                : null;

            if (!text) continue;

            text.text = hasOffer
                ? GetOfferButtonText(offers[i])
                : string.Empty;
        }

        if (selectionLabel)
            selectionLabel.text = "Ability Offers";

        if (descriptionLabel)
        {
            descriptionLabel.text = IsValidOfferIndex(offers, selectedOfferIndex)
                ? GetOfferDescriptionText(offers[selectedOfferIndex])
                : "No offers available.";
        }
    }

    private void HandleOfferButtonClicked(Button clickedButton)
    {
        if (!buttonIndexes.TryGetValue(clickedButton, out var buttonIndex)) return;
        OfferClicked?.Invoke(buttonIndex);
    }

    private void CacheReferences()
    {
        if (!selectionLabel)
            selectionLabel = FindTextByName("Selection_label");

        if (!descriptionLabel)
            descriptionLabel = FindTextByName("Ability_desctiption");

        if (!selectionButtonsRoot)
            selectionButtonsRoot = FindChildByName("Selection_buttons");

        if (offerButtons == null || offerButtons.Length == 0)
        {
            offerButtons = selectionButtonsRoot
                ? selectionButtonsRoot.GetComponentsInChildren<Button>(true)
                : Array.Empty<Button>();
        }

        if (offerButtonLabels == null || offerButtonLabels.Length != offerButtons.Length)
        {
            offerButtonLabels = new TMP_Text[offerButtons.Length];

            for (var i = 0; i < offerButtons.Length; i++)
            {
                var button = offerButtons[i];
                offerButtonLabels[i] = button
                    ? button.GetComponentInChildren<TMP_Text>(true)
                    : null;
            }
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

    private TMP_Text FindTextByName(string objectName)
    {
        var textComponents = GetComponentsInChildren<TMP_Text>(true);
        for (var i = 0; i < textComponents.Length; i++)
        {
            var textComponent = textComponents[i];
            if (textComponent && textComponent.name == objectName)
                return textComponent;
        }

        return null;
    }

    private Transform FindChildByName(string objectName)
    {
        var transforms = GetComponentsInChildren<Transform>(true);
        for (var i = 0; i < transforms.Length; i++)
        {
            var child = transforms[i];
            if (child != null && child.name == objectName)
                return child;
        }

        return null;
    }

    private static string GetOfferButtonText(AbilityOffer offer)
    {
        var kindLabel = offer.Kind == AbilityOfferKind.Upgrade ? "Upgrade" : "New";
        var name = offer.Config ? offer.Config.DisplayName : "Missing Ability";
        return $"{kindLabel}: {name}";
    }

    private static string GetOfferDescriptionText(AbilityOffer offer)
    {
        var abilityName = offer.Config ? offer.Config.DisplayName : "Missing Ability";
        var description = offer.Config ? offer.Config.Description : string.Empty;
        var kindLabel = offer.Kind == AbilityOfferKind.Upgrade ? "Upgrade" : "New Ability";

        if (string.IsNullOrWhiteSpace(description))
            return $"{kindLabel}\n{abilityName}";

        return $"{kindLabel}\n{abilityName}\n\n{description}";
    }

    private static bool IsValidOfferIndex(IReadOnlyList<AbilityOffer> offers, int index)
    {
        return offers != null && index >= 0 && index < offers.Count;
    }
}
