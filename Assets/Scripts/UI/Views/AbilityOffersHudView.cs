using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AbilityOffersHudView : MonoBehaviour
{
    [SerializeField] private Button openAbilityOffersButton;
    [SerializeField] private TextMeshProUGUI availablePointsText;

    private bool isInitialized;
    private bool isSubscribed;

    public event Action OpenOffersButtonClicked;

    public void Initialize()
    {
        if (isInitialized) return;

        ValidateReferences();
        SetAbilityMenuButtonEnabled(false);
        isInitialized = true;

        if (isActiveAndEnabled)
            SubscribeToButton();
    }

    public void SetAvailableAmount(int amount)
    {
        if (!availablePointsText) return;

        var clampedAmount = Mathf.Max(0, amount);
        availablePointsText.text = clampedAmount.ToString();
    }

    public void SetAbilityMenuButtonEnabled(bool isEnabled)
    {
        if (!openAbilityOffersButton) return;
        openAbilityOffersButton.interactable = isEnabled;
    }

    private void OnEnable()
    {
        if (!isInitialized) return;
        SubscribeToButton();
    }

    private void OnDisable()
    {
        if (!isInitialized) return;
        UnsubscribeFromButton();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void SubscribeToButton()
    {
        if (isSubscribed) return;
        if (!openAbilityOffersButton) return;

        openAbilityOffersButton.onClick.AddListener(HandleButtonClicked);
        isSubscribed = true;
    }

    private void UnsubscribeFromButton()
    {
        if (!isSubscribed) return;
        if (!openAbilityOffersButton) return;

        openAbilityOffersButton.onClick.RemoveListener(HandleButtonClicked);
        isSubscribed = false;
    }

    private void HandleButtonClicked()
    {
        OpenOffersButtonClicked?.Invoke();
    }

    private void ValidateReferences()
    {
        if (!openAbilityOffersButton)
            Debug.LogError($"{nameof(AbilityOffersHudView)} on {name} requires an offers button reference.", this);

        if (!availablePointsText)
            Debug.LogError($"{nameof(AbilityOffersHudView)} on {name} requires an available points text reference.", this);
    }
}
