using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Puck : MonoBehaviour
{
    [SerializeField] private float _startForce = 30f;
    [SerializeField] private float _shakeForce = 8f;

    private Rigidbody2D _rb;
    private Vector2 _startPosition;

    private void Awake()
    {
        _rb ??= GetComponent<Rigidbody2D>();
        _startPosition = _rb.position;
    }

    private void Start()
    {
        LaunchRandom(_startForce);
    }

    public void ResetPuck()
    {
#if UNITY_6000_0_OR_NEWER
        _rb.linearVelocity = Vector2.zero;
#else
        _rb.velocity = Vector2.zero;
#endif
        _rb.angularVelocity = 0f;
        _rb.position = _startPosition;
    }

    public void LaunchRandom(float force)
    {
        var direction = Random.value > 0.5f ? Vector2.left : Vector2.right;
        direction.y = Random.Range(-0.4f, 0.4f);

        _rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }
}