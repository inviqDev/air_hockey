using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class PuckPoofParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem collisionPoof;
    [SerializeField] private int emitCount = 8;
    [SerializeField] private float repeatDelay = 0.03f;

    private float nextAllowedPoofTime;

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
            Debug.LogError($"{nameof(PuckPoofParticles)} on {name} requires a ParticleSystem reference.", this);
    }

    public void Play(Vector2 contactPoint)
    {
        if (!collisionPoof) return;
        if (Time.time < nextAllowedPoofTime) return;

        var emitParams = new ParticleSystem.EmitParams
        {
            position = new Vector3(contactPoint.x, contactPoint.y, transform.position.z)
        };

        collisionPoof.Emit(emitParams, emitCount);
        nextAllowedPoofTime = Time.time + repeatDelay;
    }

    private void OnValidate()
    {
        if (emitCount < 1)
            emitCount = 1;

        if (repeatDelay < 0f)
            repeatDelay = 0f;
    }
}
