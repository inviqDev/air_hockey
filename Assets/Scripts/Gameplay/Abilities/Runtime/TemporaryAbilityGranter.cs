using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TemporaryAbilityGranter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAbilityController playerAbilityController;
    [SerializeField] private DashAbilityConfig dashAbilityConfig;

    [Header("Grant")]
    [SerializeField, Range(1, 4)] private int slotNumber = 1;
    [SerializeField] private bool grantOnStart = true;
    [SerializeField] private Key grantKey = Key.None;

    private void Reset()
    {
        if (!playerAbilityController)
            playerAbilityController = GetComponent<PlayerAbilityController>();

        if (!playerAbilityController)
            playerAbilityController = GetComponentInParent<PlayerAbilityController>();
    }

    private void Start()
    {
        if (!grantOnStart) return;

        GrantAbility();
    }

    private void Update()
    {
        if (grantKey == Key.None) return;
        if (Keyboard.current == null) return;
        if (!Keyboard.current[grantKey].wasPressedThisFrame) return;

        GrantAbility();
    }

    private void GrantAbility()
    {
        if (!playerAbilityController)
        {
            Debug.LogError($"{nameof(TemporaryAbilityGranter)} on {name} requires a {nameof(PlayerAbilityController)} reference.", this);
            return;
        }

        if (!dashAbilityConfig)
        {
            Debug.LogError($"{nameof(TemporaryAbilityGranter)} on {name} requires a {nameof(DashAbilityConfig)} reference.", this);
            return;
        }

        var slotIndex = slotNumber - 1;
        playerAbilityController.AddAbilityToSlot(dashAbilityConfig, slotIndex);
    }
}
