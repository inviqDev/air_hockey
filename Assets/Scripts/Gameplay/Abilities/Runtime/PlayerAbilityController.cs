using UnityEngine;

public sealed class PlayerAbilityController : MonoBehaviour
{
    private const int SlotCount = 4;

    private readonly IAbility[] abilitySlots = new IAbility[SlotCount];

    public void SetAbilityToSlot(IAbility ability, int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return;

        var previousAbility = abilitySlots[slotIndex];
        if (previousAbility == ability) return;

        previousAbility?.Dispose();
        abilitySlots[slotIndex] = ability;
    }

    public void UseSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return;

        var ability = abilitySlots[slotIndex];
        if (ability == null) return;
        if (!ability.CanActivate) return;

        ability.Activate();
    }

    public IAbility GetAbilityInSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return null;

        return abilitySlots[slotIndex];
    }

    private void FixedUpdate()
    {
        var deltaTime = Time.fixedDeltaTime;

        for (var i = 0; i < abilitySlots.Length; i++)
        {
            var ability = abilitySlots[i];
            if (ability == null) continue;

            ability.Tick(deltaTime);
        }
    }

    private void OnDestroy()
    {
        DisposeAbilities();
    }

    private void DisposeAbilities()
    {
        for (var i = 0; i < abilitySlots.Length; i++)
        {
            abilitySlots[i]?.Dispose();
            abilitySlots[i] = null;
        }
    }

    private static bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < SlotCount;
    }
}
