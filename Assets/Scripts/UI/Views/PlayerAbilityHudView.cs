using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerAbilityHudView : MonoBehaviour
{
    [Header("Free Ability Timer")]
    [SerializeField] private TextMeshProUGUI freeAbilityTimerText;
    
    [Header("Add Ability Button")]
    [SerializeField] private Button addAbilityButton;
    [SerializeField] private TextMeshProUGUI availableAmountText;

    public void SetFreeAbilityTimerText(string value)
    {
        if (!freeAbilityTimerText) return;
        freeAbilityTimerText.text = value;
    }

    public void SetAvailableAmount(int amount)
    {
        if (!availableAmountText) return;

        var clampedAmount = Mathf.Max(0, amount);
        availableAmountText.text = clampedAmount.ToString();

        if (!addAbilityButton) return;

        addAbilityButton.interactable = clampedAmount > 0;
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void Awake()
    {
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (!freeAbilityTimerText)
            Debug.LogError($"{nameof(PlayerAbilityHudView)} on {name} requires a free ability timer text reference.", this);

        if (!addAbilityButton)
            Debug.LogError($"{nameof(PlayerAbilityHudView)} on {name} requires an add ability button reference.", this);

        if (!availableAmountText)
            Debug.LogError($"{nameof(PlayerAbilityHudView)} on {name} requires an available amount text reference.", this);
    }
}
