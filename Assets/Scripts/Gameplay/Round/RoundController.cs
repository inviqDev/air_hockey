using UnityEngine;

public sealed class RoundController : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private GameObject tableRoot;

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
    [SerializeField] private ServeManager serveManager;
    [SerializeField] private PuckRegistry puckRegistry;

    private Rigidbody2D _puck;
    private Rigidbody2D _leftStriker;
    private Rigidbody2D _rightStriker;
    
    public void SpawnGameItems(MatchConfiguration configuration)
    {
        DespawnGameItems();
        ShowTable();

        _puck = SpawnRigidbody(puckPrefab, GetPosition(leftPuckDefaultPoint));
        puckRegistry?.RegisterPuck(_puck);

        _leftStriker = SpawnStriker(configuration, PlayerSide.Left, GetPosition(leftStrikerDefaultPoint));
        _rightStriker = SpawnStriker(configuration, PlayerSide.Right, GetPosition(rightStrikerDefaultPoint));

        ResetRound();
    }

    public void DespawnGameItems()
    {
        Debug.Log("Add pool");

        DestroyBody(_leftStriker);
        DestroyBody(_rightStriker);
        DestroyBody(_puck);

        _leftStriker = null;
        _rightStriker = null;
        _puck = null;
        puckRegistry?.Clear();

        HideTable();
    }

    public void ResetRound()
    {
        ResetBody(_leftStriker, GetPosition(leftStrikerDefaultPoint));
        ResetBody(_rightStriker, GetPosition(rightStrikerDefaultPoint));

        if (serveManager)
        {
            ResetBody(_puck, serveManager.GetPuckStartPosition(GetPosition(leftPuckDefaultPoint), GetPosition(rightPuckDefaultPoint)));
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
        if (!prefab) return null;

        var instance = Instantiate(prefab, position, Quaternion.identity);
        return instance.GetComponent<Rigidbody2D>();
    }

    private Rigidbody2D SpawnStriker(MatchConfiguration configuration, PlayerSide side, Vector2 position)
    {
        var player = configuration.GetPlayerForSide(side);
        var isAi = player == MatchPlayer.PlayerTwo &&
                   configuration.PlayerTwoControlType == PlayerTwoControlType.Ai;

        var striker = SpawnRigidbody(isAi ? leftAiStrikerPrefab : rightPlayerStrikerPrefab, position);

        if (isAi)
        {
            ConfigureAiStriker(striker, side);
            return striker;
        }

        var controlScheme = GetControlScheme(configuration, player, side);

        ConfigurePlayerStriker(striker, side, controlScheme);
        return striker;
    }

    private static PlayerControlScheme GetControlScheme(MatchConfiguration configuration, MatchPlayer player, PlayerSide side)
    {
        if (configuration.PlayerTwoControlType == PlayerTwoControlType.Ai && player == MatchPlayer.PlayerOne)
        {
            return PlayerControlScheme.WasdAndArrows;
        }

        return side == PlayerSide.Left
            ? PlayerControlScheme.Wasd
            : PlayerControlScheme.Arrows;
    }

    private void ShowTable()
    {
        if (tableRoot)
            tableRoot.SetActive(true);
    }

    private void HideTable()
    {
        if (tableRoot)
            tableRoot.SetActive(false);
    }

    private static void ConfigurePlayerStriker(Rigidbody2D striker, PlayerSide side, PlayerControlScheme controlScheme)
    {
        if (!striker) return;

        ConfigureSideOwner(striker, side);

        var inputSource = striker.GetComponent<PlayerInputCommandSource>();
        inputSource?.SetControlScheme(controlScheme);
    }

    private void ConfigureAiStriker(Rigidbody2D striker, PlayerSide side)
    {
        if (!striker) return;

        ConfigureSideOwner(striker, side);

        var aiCommandSource = striker.GetComponent<AICommandSource>();
        aiCommandSource?.SetSide(side);
        aiCommandSource?.SetPuck(_puck);
    }

    private static void ConfigureSideOwner(Rigidbody2D striker, PlayerSide side)
    {
        var sideOwner = striker.GetComponent<SideOwner>();
        if (sideOwner != null)
        {
            sideOwner.Side = side;
        }
    }

    private static Vector2 GetPosition(Transform point)
    {
        return point ? point.position : Vector2.zero;
    }

    private static void DestroyBody(Rigidbody2D body)
    {
        if (!body) return;

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
        if (tableRoot == null)
        {
            Debug.LogError($"{nameof(RoundController)} requires a table root reference.", this);
        }

        if (puckPrefab == null)
        {
            Debug.LogError($"{nameof(RoundController)} requires a _puck prefab reference.", this);
        }

        if (leftAiStrikerPrefab == null)
        {
            Debug.LogError($"{nameof(RoundController)} requires a left AI striker prefab reference.", this);
        }

        if (rightPlayerStrikerPrefab == null)
        {
            Debug.LogError($"{nameof(RoundController)} requires a right player striker prefab reference.", this);
        }

        if (leftPuckDefaultPoint == null)
        {
            Debug.LogError($"{nameof(RoundController)} requires a left _puck default point reference.", this);
        }

        if (rightPuckDefaultPoint == null)
        {
            Debug.LogError($"{nameof(RoundController)} requires a right _puck default point reference.", this);
        }

        if (leftStrikerDefaultPoint == null)
        {
            Debug.LogError($"{nameof(RoundController)} requires a left striker default point reference.", this);
        }

        if (rightStrikerDefaultPoint == null)
        {
            Debug.LogError($"{nameof(RoundController)} requires a right striker default point reference.", this);
        }

        if (serveManager == null)
        {
            Debug.LogError($"{nameof(RoundController)} requires a ServeManager reference.", this);
        }

        if (puckRegistry == null)
        {
            Debug.LogError($"{nameof(RoundController)} requires a PuckRegistry reference.", this);
        }
    }
}