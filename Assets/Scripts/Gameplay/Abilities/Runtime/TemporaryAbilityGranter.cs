using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TemporaryAbilityGranter : MonoBehaviour
{
    [System.Serializable]
    private sealed class AbilityGrant
    {
        [SerializeField] private AbilityConfig abilityConfig;
        [SerializeField, Range(1, 4)] private int slotNumber = 1;

        public AbilityConfig AbilityConfig => abilityConfig;
        public int SlotIndex => slotNumber - 1;
    }

    [Header("References")]
    [SerializeField] private PlayerAbilityController playerAbilityController;

    [Header("Abilities")]
    [SerializeField] private AbilityGrant[] abilityGrants;

    [Header("Grant")]
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

        GrantAbilities();
    }

    private void Update()
    {
        if (grantKey == Key.None) return;
        if (Keyboard.current == null) return;
        if (!Keyboard.current[grantKey].wasPressedThisFrame) return;

        GrantAbilities();
    }

    private void GrantAbilities()
    {
        if (!playerAbilityController)
        {
            Debug.LogError($"{nameof(TemporaryAbilityGranter)} on {name} requires a {nameof(PlayerAbilityController)} reference.", this);
            return;
        }

        if (HasAbilityGrants())
        {
            GrantConfiguredAbilities();
            return;
        }

        Debug.LogError($"{nameof(TemporaryAbilityGranter)} on {name} requires at least one ability grant.", this);
    }

    private void GrantConfiguredAbilities()
    {
        for (var i = 0; i < abilityGrants.Length; i++)
        {
            var grant = abilityGrants[i];
            if (grant == null || !grant.AbilityConfig) continue;

            playerAbilityController.AddAbilityToSlot(grant.AbilityConfig, grant.SlotIndex);
        }
    }

    private bool HasAbilityGrants()
    {
        return abilityGrants != null && abilityGrants.Length > 0;
    }
}
