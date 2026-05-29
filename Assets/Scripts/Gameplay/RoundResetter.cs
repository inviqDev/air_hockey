using UnityEngine;

public sealed class RoundResetter : MonoBehaviour
{
    [SerializeField] private Rigidbody2D puck;
    [SerializeField] private Rigidbody2D leftStriker;
    [SerializeField] private Rigidbody2D rightStriker;
    [SerializeField] private ServeManager serveManager;

    [Header("Start Positions")]
    [SerializeField] private Vector2 leftStrikerStartPosition = new(-8f, 0f);
    [SerializeField] private Vector2 rightStrikerStartPosition = new(8f, 0f);

    public bool IsPuck(Rigidbody2D candidate)
    {
        return candidate && candidate == puck;
    }

    public void ResetRound()
    {
        ResetBody(leftStriker, leftStrikerStartPosition);
        ResetBody(rightStriker, rightStrikerStartPosition);

        if (serveManager)
        {
            ResetBody(puck, serveManager.GetPuckStartPosition(leftStrikerStartPosition, rightStrikerStartPosition));
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void Awake()
    {
        ValidateReferences();
    }

    private static void ResetBody(Rigidbody2D body, Vector2 position)
    {
        if (!body) return;

#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = Vector2.zero;
#else
    body.velocity = Vector2.zero;
#endif
        
        body.angularVelocity = 0f;
        body.position = position;
        body.rotation = 0f;
    }

    private void ValidateReferences()
    {
        if (puck == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a puck Rigidbody2D reference.", this);
        }

        if (leftStriker == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a left striker Rigidbody2D reference.", this);
        }

        if (rightStriker == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a right striker Rigidbody2D reference.", this);
        }

        if (serveManager == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a ServeManager reference.", this);
        }
    }
}
