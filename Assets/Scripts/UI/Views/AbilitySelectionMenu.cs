using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class AbilitySelectionMenu : MonoBehaviour
{
    [Header("Optional References")]
    [SerializeField] private TMP_Text selectionLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private Transform selectionButtonsRoot;
    [SerializeField] private Button[] offerButtons;

    public event Action<AbilityOffer> OfferSelected;

    public bool IsOpen => gameObject.activeSelf;

    private readonly List<AbilityOffer> currentOffers = new(3);
    private readonly Dictionary<Button, int> buttonIndexes = new();
    private readonly Dictionary<Button, UnityEngine.Events.UnityAction> buttonClickHandlers = new();
    private int selectedOfferIndex = -1;

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
        selectedOfferIndex = -1;
    }

    private void OnValidate()
    {
        CacheReferences();
        RebuildButtonIndexMap();
    }

    private void Update()
    {
        if (!IsOpen) return;

        var selectedObject = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        if (!selectedObject) return;

        if (!selectedObject.TryGetComponent(out Button selectedButton)) return;
        if (!buttonIndexes.TryGetValue(selectedButton, out var buttonIndex)) return;
        if (buttonIndex >= currentOffers.Count) return;
        if (selectedOfferIndex == buttonIndex) return;

        SelectOffer(buttonIndex, false);
    }

    public void ShowOffers(IReadOnlyList<AbilityOffer> offers)
    {
        CacheReferences();
        currentOffers.Clear();

        if (offers != null)
        {
            for (var i = 0; i < offers.Count; i++)
            {
                currentOffers.Add(offers[i]);
            }
        }

        gameObject.SetActive(true);
        ApplyOffers();
        SelectFirstAvailableOffer();
    }

    public void Close()
    {
        selectedOfferIndex = -1;
        gameObject.SetActive(false);
    }

    public void Toggle(IReadOnlyList<AbilityOffer> offers)
    {
        if (IsOpen)
        {
            Close();
            return;
        }

        ShowOffers(offers);
    }

    private void ApplyOffers()
    {
        if (selectionLabel)
            selectionLabel.text = "Ability Offers";

        for (var i = 0; i < offerButtons.Length; i++)
        {
            var hasOffer = i < currentOffers.Count;
            var button = offerButtons[i];
            if (!button) continue;

            button.gameObject.SetActive(hasOffer);
            button.interactable = hasOffer;

            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (!text) continue;

            text.text = hasOffer
                ? GetOfferButtonText(currentOffers[i])
                : string.Empty;
        }

        if (descriptionLabel)
        {
            descriptionLabel.text = currentOffers.Count > 0
                ? GetOfferDescriptionText(currentOffers[0])
                : "No offers available.";
        }
    }

    private void SelectFirstAvailableOffer()
    {
        selectedOfferIndex = -1;

        for (var i = 0; i < offerButtons.Length; i++)
        {
            var button = offerButtons[i];
            if (!button) continue;
            if (!button.gameObject.activeInHierarchy) continue;
            if (!button.interactable) continue;

            SelectOffer(i, false);

            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject(button.gameObject);

            return;
        }

        if (EventSystem.current)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void SelectOffer(int buttonIndex, bool notifySelection)
    {
        if (buttonIndex < 0 || buttonIndex >= currentOffers.Count) return;

        selectedOfferIndex = buttonIndex;

        if (descriptionLabel)
            descriptionLabel.text = GetOfferDescriptionText(currentOffers[buttonIndex]);

        if (!notifySelection) return;

        OfferSelected?.Invoke(currentOffers[buttonIndex]);
    }

    private void HandleOfferButtonClicked(Button clickedButton)
    {
        if (!buttonIndexes.TryGetValue(clickedButton, out var buttonIndex)) return;
        SelectOffer(buttonIndex, true);
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
}
