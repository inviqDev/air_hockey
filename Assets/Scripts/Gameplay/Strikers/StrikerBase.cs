using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SideOwner))]
[RequireComponent(typeof(DashAbility))]
public abstract class StrikerBase : MonoBehaviour
{
    [SerializeField] private SideOwner sideOwner;
    [SerializeField] private DashAbility dashAbility;

    private Rigidbody2D strikerRigidbody;

    private void Reset()
    {
        if (!strikerRigidbody)
            strikerRigidbody = GetComponent<Rigidbody2D>();
        
        if (!sideOwner)
            sideOwner = GetComponent<SideOwner>();

        if (!dashAbility)
            dashAbility = GetComponent<DashAbility>();
    }

    private void Awake()
    {
        CacheReferences();
    }

    public void Initialize(StrikerSetupContext setupContext)
    {
        CacheReferences();

        if (sideOwner)
            sideOwner.Side = setupContext.Side;

        ApplyStrikerSetup(setupContext);
    }

    public void ResetState(Vector2 position)
    {
        CacheReferences();

        if (!strikerRigidbody) return;

#if UNITY_6000_0_OR_NEWER
        strikerRigidbody.linearVelocity = Vector2.zero;
#else
        strikerRigidbody.velocity = Vector2.zero;
#endif

        strikerRigidbody.angularVelocity = 0f;
        strikerRigidbody.position = position;
        strikerRigidbody.rotation = 0f;

        if (dashAbility)
            dashAbility.ResetState();

        ResetCustomStrikerState();
    }

    protected virtual void ResetCustomStrikerState()
    {
    }

    protected abstract void ApplyStrikerSetup(StrikerSetupContext setupContext);

    private void CacheReferences()
    {
        if (!sideOwner)
            sideOwner = GetComponent<SideOwner>();

        if (!dashAbility)
            dashAbility = GetComponent<DashAbility>();
    }
}