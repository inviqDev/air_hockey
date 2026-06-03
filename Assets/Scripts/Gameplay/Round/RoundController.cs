using UnityEngine;
using UnityEngine.Serialization;

public sealed class RoundController : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private GameObject tableRoot;

    [Header("Prefabs")]
    [SerializeField] private Puck puckPrefab;
    [SerializeField] private MovementMotor2D aiStrikerPrefab;
    [SerializeField] private MovementMotor2D playerStrikerPrefab;

    [Header("Default Points")]
    [SerializeField] private Transform leftPuckSpawnPoint;
    [SerializeField] private Transform rightPuckSpawnPoint;
    [SerializeField] private Transform leftStrikerSpawnPoint;
    [SerializeField] private Transform rightStrikerSpawnPoint;

    [Header("Runtime Instances")]
    [SerializeField] private ServeManager serveManager;
    [SerializeField] private PuckRegistry puckRegistry;

    private Puck puck;
    private Rigidbody2D leftStrikerRb;
    private Rigidbody2D rightStrikerRb;

    public void SpawnGameItems(MatchConfiguration configuration)
    {
        DespawnGameItems();
        ShowTable();

        puck = SpawnPuck(puckPrefab, GetPosition(leftPuckSpawnPoint));
        puckRegistry?.RegisterPuck(puck);

        leftStrikerRb = SpawnStriker(configuration, PlayerSide.Left, GetPosition(leftStrikerSpawnPoint));
        rightStrikerRb = SpawnStriker(configuration, PlayerSide.Right, GetPosition(rightStrikerSpawnPoint));

        ResetRound();
    }

    public void DespawnGameItems()
    {
        Debug.Log("Add pool");

        DestroyBody(leftStrikerRb);
        DestroyBody(rightStrikerRb);
        DestroyPuck(puck);

        leftStrikerRb = null;
        rightStrikerRb = null;
        puck = null;
        puckRegistry?.Clear();

        HideTable();
    }

    public void ResetRound()
    {
        ResetBody(leftStrikerRb, GetPosition(leftStrikerSpawnPoint));
        ResetBody(rightStrikerRb, GetPosition(rightStrikerSpawnPoint));

        if (serveManager)
            ResetPuck(puck, serveManager.GetPuckStartPosition(GetPosition(leftPuckSpawnPoint), GetPosition(rightPuckSpawnPoint)));
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

    private static Rigidbody2D SpawnStrikerBody(MovementMotor2D prefab, Vector2 position)
    {
        if (!prefab) return null;

        var instance = Instantiate(prefab, position, Quaternion.identity);
        return instance.GetComponent<Rigidbody2D>();
    }

    private static Puck SpawnPuck(Puck prefab, Vector2 position)
    {
        if (!prefab) return null;

        var instance = Instantiate(prefab, position, Quaternion.identity);
        return instance;
    }

    private Rigidbody2D SpawnStriker(MatchConfiguration configuration, PlayerSide side, Vector2 position)
    {
        var player = configuration.GetPlayerForSide(side);
        var isAi = player == MatchPlayer.PlayerTwo &&
                   configuration.PlayerTwoControlType == PlayerTwoControlType.Ai;

        var strikerPrefab = isAi ? aiStrikerPrefab : playerStrikerPrefab;
        var striker = SpawnStrikerBody(strikerPrefab, position);

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
            return PlayerControlScheme.WasdAndArrows;

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
        aiCommandSource?.SetPuck(puck);
    }

    private static void ConfigureSideOwner(Rigidbody2D striker, PlayerSide side)
    {
        var sideOwner = striker.GetComponent<SideOwner>();
        if (sideOwner)
            sideOwner.Side = side;
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

    private static void DestroyPuck(Puck puck)
    {
        if (!puck) return;

        Destroy(puck.gameObject);
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

    private static void ResetPuck(Puck puck, Vector2 position)
    {
        if (!puck) return;

        puck.ResetState(position);
    }

    private void ValidateReferences()
    {
        if (!tableRoot)
            Debug.LogError($"{nameof(RoundController)} requires a table root reference.", this);

        if (!puckPrefab)
            Debug.LogError($"{nameof(RoundController)} requires a puck prefab reference.", this);

        if (!aiStrikerPrefab)
            Debug.LogError($"{nameof(RoundController)} requires a AI striker prefab reference.", this);

        if (!playerStrikerPrefab)
            Debug.LogError($"{nameof(RoundController)} requires a player striker prefab reference.", this);

        if (!leftPuckSpawnPoint)
            Debug.LogError($"{nameof(RoundController)} requires a left puck spawn point reference.", this);

        if (!rightPuckSpawnPoint)
            Debug.LogError($"{nameof(RoundController)} requires a right puck spawn point reference.", this);

        if (!leftStrikerSpawnPoint)
            Debug.LogError($"{nameof(RoundController)} requires a left striker spawn point reference.", this);

        if (!rightStrikerSpawnPoint)
            Debug.LogError($"{nameof(RoundController)} requires a right striker spawn point reference.", this);

        if (!serveManager)
            Debug.LogError($"{nameof(RoundController)} requires a ServeManager reference.", this);

        if (!puckRegistry)
            Debug.LogError($"{nameof(RoundController)} requires a PuckRegistry reference.", this);
    }
}
