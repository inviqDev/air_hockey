using UnityEngine;

public sealed class RoundController : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private GameObject tableRoot;

    [Header("Prefabs")]
    [SerializeField] private Puck puckPrefab;
    [SerializeField] private AiStriker aiStrikerPrefab;
    [SerializeField] private PlayerStriker playerStrikerPrefab;

    [Header("Default Points")]
    [SerializeField] private Transform leftPuckSpawnPoint;
    [SerializeField] private Transform rightPuckSpawnPoint;
    [SerializeField] private Transform leftStrikerSpawnPoint;
    [SerializeField] private Transform rightStrikerSpawnPoint;

    [Header("Runtime Instances")]
    [SerializeField] private TurnController turnController;
    [SerializeField] private ServeManager serveManager;
    [SerializeField] private PuckRegistry puckRegistry;

    private Puck puck;
    private StrikerBase leftStriker;
    private StrikerBase rightStriker;

    public bool HasAllGameItemsSpawned =>
        IsGameplayItemReady(puck) &&
        IsGameplayItemReady(leftStriker) &&
        IsGameplayItemReady(rightStriker);

    public bool SpawnGameItems(MatchConfiguration configuration)
    {
        DespawnGameItems();
        ShowTable();

        puck = SpawnPuck(puckPrefab, GetPosition(leftPuckSpawnPoint));
        if (puckRegistry)
            puckRegistry.RegisterPuck(puck);

        leftStriker = SpawnStriker(configuration, PlayerSide.Left, GetPosition(leftStrikerSpawnPoint));
        rightStriker = SpawnStriker(configuration, PlayerSide.Right, GetPosition(rightStrikerSpawnPoint));

        ResetRound();
        return HasAllGameItemsSpawned;
    }

    public bool PrepareTurn()
    {
        if (!HasAllGameItemsSpawned) return false;

        ResetRound();
        return HasAllGameItemsSpawned;
    }

    public bool RespawnTurnItems(MatchConfiguration configuration)
    {
        return SpawnGameItems(configuration);
    }
    
    public void DespawnGameItems()
    {
        DestroyStriker(leftStriker);
        DestroyStriker(rightStriker);
        DestroyPuck(puck);

        leftStriker = null;
        rightStriker = null;
        puck = null;

        if (puckRegistry)
            puckRegistry.Clear();

        HideTable();
    }

    public void ResetRound()
    {
        ResetStriker(leftStriker, GetPosition(leftStrikerSpawnPoint));
        ResetStriker(rightStriker, GetPosition(rightStrikerSpawnPoint));

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

    private static T SpawnGameplayItem<T>(T prefab, Vector2 position) where T : Component
    {
        if (!prefab) return null;
        
        return  Instantiate(prefab, position, Quaternion.identity);
    }

    private static Puck SpawnPuck(Puck prefab, Vector2 position)
    {
        return SpawnGameplayItem(prefab, position);
    }

    private StrikerBase SpawnStriker(MatchConfiguration configuration, PlayerSide side, Vector2 position)
    {
        var player = configuration.GetPlayerForSide(side);
        var isAi = player == MatchPlayer.PlayerTwo &&
                   configuration.PlayerTwoControlType == PlayerTwoControlType.Ai;

        var controlScheme = GetControlScheme(configuration, player, side);
        var setupContext = new StrikerSetupContext(side, puck, controlScheme);
        var striker = isAi
            ? SpawnGameplayItem<StrikerBase>(aiStrikerPrefab, position)
            : SpawnGameplayItem<StrikerBase>(playerStrikerPrefab, position);

        if (striker)
            striker.Initialize(setupContext, turnController);

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

    private static Vector2 GetPosition(Transform point)
    {
        return point ? point.position : Vector2.zero;
    }

    private static void DestroyStriker(StrikerBase striker)
    {
        if (!striker) return;

        Destroy(striker.gameObject);
    }

    private static void DestroyPuck(Puck puck)
    {
        if (!puck) return;

        Destroy(puck.gameObject);
    }

    private static void ResetStriker(StrikerBase striker, Vector2 position)
    {
        if (!striker) return;

        striker.ResetState(position);
    }

    private static void ResetPuck(Puck puck, Vector2 position)
    {
        if (!puck) return;

        puck.ResetState(position);
    }

    private static bool IsGameplayItemReady(Component gameplayItem)
    {
        return gameplayItem && gameplayItem.gameObject.activeInHierarchy;
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

        if (!turnController)
            Debug.LogError($"{nameof(RoundController)} requires a TurnController reference.", this);

        if (!serveManager)
            Debug.LogError($"{nameof(RoundController)} requires a ServeManager reference.", this);

        if (!puckRegistry)
            Debug.LogError($"{nameof(RoundController)} requires a PuckRegistry reference.", this);
    }
}
