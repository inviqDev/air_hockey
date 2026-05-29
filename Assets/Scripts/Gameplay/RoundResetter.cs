using UnityEngine;

public sealed class RoundResetter : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject puckPrefab;
    [SerializeField] private GameObject leftAiStrikerPrefab;
    [SerializeField] private GameObject rightPlayerStrikerPrefab;

    [Header("Default Points")]
    [SerializeField] private Transform leftPuckDefaultPoint;
    [SerializeField] private Transform rightPuckDefaultPoint;
    [SerializeField] private Transform leftStrikerDefaultPoint;
    [SerializeField] private Transform rightStrikerDefaultPoint;

    [Header("Runtime Instances")]
    [SerializeField] private Rigidbody2D puck;
    [SerializeField] private Rigidbody2D leftStriker;
    [SerializeField] private Rigidbody2D rightStriker;
    [SerializeField] private ServeManager serveManager;

    public bool IsPuck(Rigidbody2D candidate)
    {
        return candidate && candidate == puck;
    }

    public void SpawnGameItemsForAiOpponent()
    {
        DespawnGameItems();

        puck = SpawnRigidbody(puckPrefab, GetPosition(leftPuckDefaultPoint));
        leftStriker = SpawnRigidbody(leftAiStrikerPrefab, GetPosition(leftStrikerDefaultPoint));
        rightStriker = SpawnRigidbody(rightPlayerStrikerPrefab, GetPosition(rightStrikerDefaultPoint));

        if (leftStriker != null)
        {
            var aiCommandSource = leftStriker.GetComponent<AICommandSource>();
            aiCommandSource?.SetPuck(puck);
        }

        ResetRound();
    }

    public void DespawnGameItems()
    {
        DestroyBody(leftStriker);
        DestroyBody(rightStriker);
        DestroyBody(puck);

        leftStriker = null;
        rightStriker = null;
        puck = null;
    }

    public void ResetRound()
    {
        ResetBody(leftStriker, GetPosition(leftStrikerDefaultPoint));
        ResetBody(rightStriker, GetPosition(rightStrikerDefaultPoint));

        if (serveManager)
        {
            ResetBody(puck, serveManager.GetPuckStartPosition(GetPosition(leftPuckDefaultPoint), GetPosition(rightPuckDefaultPoint)));
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void Awake()
    {
        ValidateReferences();
        DespawnGameItems();
    }

    private static Rigidbody2D SpawnRigidbody(GameObject prefab, Vector2 position)
    {
        if (prefab == null)
        {
            return null;
        }

        var instance = Instantiate(prefab, position, Quaternion.identity);
        return instance.GetComponent<Rigidbody2D>();
    }

    private static Vector2 GetPosition(Transform point)
    {
        return point ? point.position : Vector2.zero;
    }

    private static void DestroyBody(Rigidbody2D body)
    {
        if (!body)
        {
            return;
        }

        Destroy(body.gameObject);
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
        if (puckPrefab == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a puck prefab reference.", this);
        }

        if (leftAiStrikerPrefab == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a left AI striker prefab reference.", this);
        }

        if (rightPlayerStrikerPrefab == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a right player striker prefab reference.", this);
        }

        if (leftPuckDefaultPoint == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a left puck default point reference.", this);
        }

        if (rightPuckDefaultPoint == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a right puck default point reference.", this);
        }

        if (leftStrikerDefaultPoint == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a left striker default point reference.", this);
        }

        if (rightStrikerDefaultPoint == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a right striker default point reference.", this);
        }

        if (serveManager == null)
        {
            Debug.LogError($"{nameof(RoundResetter)} requires a ServeManager reference.", this);
        }
    }
}
