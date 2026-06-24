using TMPro;
using UnityEngine;

public sealed class FreeAbilityTimerHudView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI freeTimerText;

    private bool isInitialized;

    public void Initialize()
    {
        if (isInitialized) return;

        ValidateReferences();
        isInitialized = true;
    }

    public void SetFreeAbilityTimerText(string value)
    {
        if (!freeTimerText) return;
        freeTimerText.text = value;
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (!freeTimerText)
            Debug.LogError($"{nameof(FreeAbilityTimerHudView)} on {name} requires a free ability timer text reference.", this);
    }
}
