using UnityEngine;
using UnityEngine.UI;

public sealed class AbilitySlotHudView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Sprite emptyAbilityIcon;
    
    [SerializeField] private Slider cooldownSlider;
    [SerializeField] private Image sliderBackground;
    [SerializeField] private Color activeBackgroundColor;
    [SerializeField] private Color inactiveBackgroundColor;

    public void SetEmpty()
    {
        SetDefault();
    }

    public void SetDefault()
    {
        SetAbilityIcon(emptyAbilityIcon);
        SetCooldown(false, 0f);
    }

    public void SetAbility(AbilityConfig config)
    {
        if (!config)
        {
            SetDefault();
            return;
        }

        var icon = config.Icon ? config.Icon : emptyAbilityIcon;
        SetAbilityIcon(icon);
    }

    public void SetCooldown(bool useActiveBackground, float normalized)
    {
        if (!cooldownSlider) return;

        var clampedNormalized = Mathf.Clamp01(normalized);
        cooldownSlider.gameObject.SetActive(true);
        cooldownSlider.value = clampedNormalized;

        if (sliderBackground)
        {
            sliderBackground.color = useActiveBackground
                ? activeBackgroundColor
                : inactiveBackgroundColor;
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void Awake()
    {
        ValidateReferences();
        SetDefault();
    }

    private void SetAbilityIcon(Sprite icon)
    {
        if (!abilityIcon) return;

        abilityIcon.sprite = icon;
        abilityIcon.enabled = icon;
    }

    private void ValidateReferences()
    {
        if (!abilityIcon)
            Debug.LogError($"{nameof(AbilitySlotHudView)} on {name} requires an ability icon image reference.", this);

        if (!emptyAbilityIcon)
            Debug.LogError($"{nameof(AbilitySlotHudView)} on {name} requires an empty ability icon sprite reference.", this);

        if (!cooldownSlider)
            Debug.LogError($"{nameof(AbilitySlotHudView)} on {name} requires a cooldown slider reference.", this);

        if (!sliderBackground)
            Debug.LogError($"{nameof(AbilitySlotHudView)} on {name} requires a cooldown background image reference.", this);
    }
}
