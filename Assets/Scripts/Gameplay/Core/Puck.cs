using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(EffectReceiver))]
public sealed class Puck : MonoBehaviour, IPoolable
{
    private const float DefaultRadius = 0.5f;

    [SerializeField] private PuckPoofParticles poofParticles;
    [SerializeField] private EffectReceiver effectReceiver;
    public Rigidbody2D PuckRigidbody { get; private set; }
    public CircleCollider2D PuckCircleCollider { get; private set; }
    public Vector2 Position => PuckRigidbody ? PuckRigidbody.position : transform.position;
    public Vector2 Velocity => PuckRigidbody ? GetLinearVelocity(PuckRigidbody) : Vector2.zero;
    public float Radius => ResolveRadius();

    private void Reset()
    {
        CacheComponents();
    }

    private void Awake()
    {
        CacheComponents();
        ValidateReferences();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayOnCollisionPoofParticles(collision);
    }

    private void FixedUpdate()
    {
        if (effectReceiver)
            effectReceiver.TickEffects(Time.fixedDeltaTime);
    }

    private void PlayOnCollisionPoofParticles(Collision2D collision)
    {
        if (!poofParticles || collision.contactCount < 1) return;
        var contactPoint = collision.GetContact(0).point;
        poofParticles.Play(contactPoint);
    }

    public void ResetState(Vector2 position)
    {
        if (!PuckRigidbody) return;

        effectReceiver?.ClearEffects();
        SetLinearVelocity(PuckRigidbody, Vector2.zero);
        PuckRigidbody.angularVelocity = 0f;
        PuckRigidbody.position = position;
        PuckRigidbody.rotation = 0f;
    }

    public void OnGetFromPool()
    {
        CacheComponents();
    }

    public void OnMoveToPool()
    {
        if (!PuckRigidbody) return;
        ResetState(PuckRigidbody.position);
    }

    private float ResolveRadius()
    {
        if (!PuckCircleCollider)
            return DefaultRadius;

        var scale = transform.lossyScale;
        var largestAxisScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        return PuckCircleCollider.radius * largestAxisScale;
    }

    private void CacheComponents()
    {
        if (!PuckRigidbody)
            PuckRigidbody = GetComponent<Rigidbody2D>();

        if (!PuckCircleCollider)
            PuckCircleCollider = GetComponent<CircleCollider2D>();

        if (!effectReceiver)
            effectReceiver = GetComponent<EffectReceiver>();

        if (!poofParticles)
            poofParticles = GetComponent<PuckPoofParticles>();
    }

    public bool TryGetEffectReceiver(out EffectReceiver receiver)
    {
        CacheComponents();
        receiver = effectReceiver;
        return receiver;
    }

    private void ValidateReferences()
    {
        if (!PuckRigidbody)
            Debug.LogError($"{nameof(Puck)} on {name} requires a Rigidbody2D reference.", this);

        if (!PuckCircleCollider)
            Debug.LogError($"{nameof(Puck)} on {name} requires a CircleCollider2D reference.", this);

        if (!effectReceiver)
            Debug.LogError($"{nameof(Puck)} on {name} requires an {nameof(EffectReceiver)} reference.", this);
    }

    private static Vector2 GetLinearVelocity(Rigidbody2D body)
    {
#if UNITY_6000_0_OR_NEWER
        return body.linearVelocity;
#else
        return body.velocity;
#endif
    }

    private static void SetLinearVelocity(Rigidbody2D body, Vector2 velocity)
    {
#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = velocity;
#else
        body.velocity = velocity;
#endif
    }
}
