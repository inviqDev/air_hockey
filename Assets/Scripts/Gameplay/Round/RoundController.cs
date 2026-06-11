using UnityEngine;

public sealed class RoundController : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private GameObject tableRoot;
    [SerializeField] private PlayersZoneLimiter playersZoneLimiter;

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

    [Header("UI")]
    [SerializeField] private AbilityBoardView leftAbilityBoard;
    [SerializeField] private AbilityBoardView rightAbilityBoard;

    private Puck puck;
    private StrikerBase leftStriker;
    private StrikerBase rightStriker;
    
    private readonly Pool pool = new();

    public bool HasAllRoundItemsActive =>
        IsRoundItemActive(puck) &&
        IsRoundItemActive(leftStriker) &&
        IsRoundItemActive(rightStriker);

    public bool ActivateRoundItems(MatchConfiguration configuration)
    {
        ReturnRoundItemsToPool();
        SetTableVisible(true);

        ActivatePuckFromPool();
        ActivateStrikersFromPool(configuration);
        RefreshAbilityBoards();
        
        ResetRoundItemsForTurn();
        
        return HasAllRoundItemsActive;
    }
    
    private void ActivatePuckFromPool()
    {
        var puckSpawnPosition = GetPosition(leftPuckSpawnPoint);
        puck = pool.TryGetFromPool(puckPrefab, puckSpawnPosition, Quaternion.identity);
            
        if (puckRegistry)
            puckRegistry.RegisterPuck(puck);
    }

    private void ActivateStrikersFromPool(MatchConfiguration configuration)
    {
        var leftStrikerSpawnPosition = GetPosition(leftStrikerSpawnPoint);
        leftStriker = ActivateStrikerFromPool(configuration, PlayerSide.Left, leftStrikerSpawnPosition);
        
        var rightStrikerSpawnPosition = GetPosition(rightStrikerSpawnPoint);
        rightStriker = ActivateStrikerFromPool(configuration, PlayerSide.Right, rightStrikerSpawnPosition);
    }

    public bool ResetRoundItemsForTurn()
    {
        if (!HasAllRoundItemsActive) return false;

        ResetRoundItemsToStartPositions();
        return HasAllRoundItemsActive;
    }

    public bool RebuildRoundItemsForTurn(MatchConfiguration configuration)
    {
        return ActivateRoundItems(configuration);
    }
    
    public void ReturnRoundItemsToPool()
    {
        ReturnItemToPool(leftStriker);
        ReturnItemToPool(rightStriker);
        ReturnItemToPool(puck);

        leftStriker = null;
        rightStriker = null;
        puck = null;

        if (puckRegistry)
            puckRegistry.Clear();

        ClearAbilityBoards();
        SetTableVisible(false);
    }

    public void ResetRoundItemsToStartPositions()
    {
        ResetStrikerToStartPosition(leftStriker, GetPosition(leftStrikerSpawnPoint));
        ResetStrikerToStartPosition(rightStriker, GetPosition(rightStrikerSpawnPoint));

        if (playersZoneLimiter)
            playersZoneLimiter.ResetState();

        if (serveManager)
            ResetPuckToServePosition(puck, serveManager.GetPuckStartPosition(GetPosition(leftPuckSpawnPoint), GetPosition(rightPuckSpawnPoint)));
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void Awake()
    {
        ValidateReferences();
        CacheAbilityBoards();
        ReturnRoundItemsToPool();
    }

    private StrikerBase ActivateStrikerFromPool(MatchConfiguration configuration, PlayerSide side, Vector2 position)
    {
        var player = configuration.GetPlayerForSide(side);
        var isAi = player == MatchPlayer.PlayerTwo &&
                   configuration.PlayerTwoControlType == PlayerTwoControlType.Ai;

        var controlScheme = GetControlScheme(configuration, player, side);
        var setupContext = new StrikerSetupContext(side, puck, controlScheme);
        
        StrikerBase striker = isAi
            ? pool.TryGetFromPool(aiStrikerPrefab, position, Quaternion.identity)
            : pool.TryGetFromPool(playerStrikerPrefab, position, Quaternion.identity);

        if (striker)
            striker.InitializeStriker(setupContext, turnController);

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

    private void SetTableVisible(bool isVisible)
    {
        if (tableRoot)
            tableRoot.SetActive(isVisible);
    }

    private static Vector2 GetPosition(Transform point)
    {
        return point ? point.position : Vector2.zero;
    }

    private void ReturnItemToPool<T>(T item) where T : Component, IPoolable
    {
        if (!item) return;
        pool.ReturnToPool(item);
    }

    private static void ResetStrikerToStartPosition(StrikerBase striker, Vector2 position)
    {
        if (!striker) return;
        striker.ResetState(position);
    }

    private static void ResetPuckToServePosition(Puck puck, Vector2 position)
    {
        if (!puck) return;
        puck.ResetState(position);
    }

    private static bool IsRoundItemActive(Component roundItem)
    {
        return roundItem && roundItem.gameObject.activeInHierarchy;
    }

    private void CacheAbilityBoards()
    {
        if (leftAbilityBoard && rightAbilityBoard)
            return;

        var boards = FindObjectsByType<AbilityBoardView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < boards.Length; i++)
        {
            if (!leftAbilityBoard && boards[i].name.Contains("Left_skill_board"))
            {
                leftAbilityBoard = boards[i];
                continue;
            }

            if (!rightAbilityBoard && boards[i].name.Contains("Right_skill_board"))
                rightAbilityBoard = boards[i];
        }
    }

    private void RefreshAbilityBoards()
    {
        CacheAbilityBoards();

        if (leftAbilityBoard)
            leftAbilityBoard.Bind(leftStriker ? leftStriker.AbilityController : null);

        if (rightAbilityBoard)
            rightAbilityBoard.Bind(rightStriker ? rightStriker.AbilityController : null);
    }

    private void ClearAbilityBoards()
    {
        CacheAbilityBoards();

        if (leftAbilityBoard)
            leftAbilityBoard.ClearBoard();

        if (rightAbilityBoard)
            rightAbilityBoard.ClearBoard();
    }

    private void ValidateReferences()
    {
        if (!tableRoot)
            Debug.LogError($"{nameof(RoundController)} requires a table root reference.", this);

        if (!puckPrefab)
            Debug.LogError($"{nameof(RoundController)} requires a puck prefab reference.", this);

        if (!playersZoneLimiter)
            Debug.LogError($"{nameof(RoundController)} requires a {nameof(PlayersZoneLimiter)} reference.", this);

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
