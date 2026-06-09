using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class Puck : MonoBehaviour, IPoolable
{
    private const float DefaultRadius = 0.5f;

    [SerializeField] private PuckPoofParticles poofParticles;
    public Rigidbody2D PuckRigidbody { get; private set; }
    public CircleCollider2D PuckCircleCollider { get; private set; }
    public Vector2 Position => PuckRigidbody ? PuckRigidbody.position : transform.position;
    public Vector2 Velocity => PuckRigidbody ? PuckRigidbody.linearVelocity : Vector2.zero;
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

    private void PlayOnCollisionPoofParticles(Collision2D collision)
    {
        if (!poofParticles || collision.contactCount < 1) return;
        var contactPoint = collision.GetContact(0).point;
        poofParticles.Play(contactPoint);
    }

    public void ResetState(Vector2 position)
    {
        if (!PuckRigidbody) return;

        PuckRigidbody.linearVelocity = Vector2.zero;
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

        if (!poofParticles)
            poofParticles = GetComponent<PuckPoofParticles>();
    }

    private void ValidateReferences()
    {
        if (!PuckRigidbody)
            Debug.LogError($"{nameof(Puck)} on {name} requires a Rigidbody2D reference.", this);

        if (!PuckCircleCollider)
            Debug.LogError($"{nameof(Puck)} on {name} requires a CircleCollider2D reference.", this);
    }
}
