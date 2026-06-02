using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class PuckCollisionPoof : MonoBehaviour
{
    [SerializeField] private ParticleSystem collisionPoof;
    [SerializeField] private int emitCount = 8;
    [SerializeField] private float minImpactSpeed = 2.2f;
    [SerializeField] private float repeatDelay = 0.03f;

    private float nextAllowedEmitTime;

    private void Reset()
    {
        if (!collisionPoof)
            collisionPoof = GetComponent<ParticleSystem>();
    }

    private void Awake()
    {
        if (!collisionPoof)
            collisionPoof = GetComponent<ParticleSystem>();

        if (!collisionPoof)
            Debug.LogError($"{nameof(PuckCollisionPoof)} on {name} requires a ParticleSystem reference.", this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collisionPoof) return;
        if (Time.time < nextAllowedEmitTime) return;
        if (collision.contactCount == 0) return;

        var minImpactSpeedSqr = minImpactSpeed * minImpactSpeed;

        if (collision.relativeVelocity.sqrMagnitude < minImpactSpeedSqr) return;

        var contactPoint = collision.GetContact(0).point;
        var emitParams = new ParticleSystem.EmitParams
        {
            position = new Vector3(contactPoint.x, contactPoint.y, transform.position.z)
        };

        collisionPoof.Emit(emitParams, emitCount);
        nextAllowedEmitTime = Time.time + repeatDelay;
    }

    private void OnValidate()
    {
        if (emitCount < 1)
            emitCount = 1;

        if (minImpactSpeed < 0f)
            minImpactSpeed = 0f;

        if (repeatDelay < 0f)
            repeatDelay = 0f;
    }
}
