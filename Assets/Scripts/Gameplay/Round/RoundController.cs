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

    [Header("Ability HUD")]
    [SerializeField] private AbilityHudView leftAbilityHud;
    [SerializeField] private AbilityHudView rightAbilityHud;

    private Puck puck;
    private StrikerBase leftStriker;
    private StrikerBase rightStriker;
    private bool isAbilityPauseStateActive;
    
    private readonly Pool gameplayItemPool = new();

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
        
        ResetRoundItemsForTurn();
        
        return HasAllRoundItemsActive;
    }
    
    private void ActivatePuckFromPool()
    {
        var puckSpawnPosition = GetPosition(leftPuckSpawnPoint);
        puck = gameplayItemPool.TryGetFromPool(puckPrefab, puckSpawnPosition, Quaternion.identity);
            
        if (puckRegistry)
            puckRegistry.RegisterPuck(puck);
    }

    private void ActivateStrikersFromPool(MatchConfiguration configuration)
    {
        var leftStrikerSpawnPosition = GetPosition(leftStrikerSpawnPoint);
        leftStriker = ActivateStrikerFromPool(configuration, PlayerSide.Left, leftStrikerSpawnPosition);
        BindAbilityHud(PlayerSide.Left, leftStriker);
        
        var rightStrikerSpawnPosition = GetPosition(rightStrikerSpawnPoint);
        rightStriker = ActivateStrikerFromPool(configuration, PlayerSide.Right, rightStrikerSpawnPosition);
        BindAbilityHud(PlayerSide.Right, rightStriker);
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

        ClearAbilityHuds();

        if (puckRegistry)
            puckRegistry.Clear();

        SetTableVisible(false);
    }

    public void SetAbilityPauseState(bool isPaused)
    {
        isAbilityPauseStateActive = isPaused;
        ApplyAbilityPauseState(leftStriker);
        ApplyAbilityPauseState(rightStriker);
    }

    public void ResetRoundItemsToStartPositions()
    {
        ResetStrikerToStartPosition(leftStriker, GetPosition(leftStrikerSpawnPoint));
        ResetStrikerToStartPosition(rightStriker, GetPosition(rightStrikerSpawnPoint));

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
            ? gameplayItemPool.TryGetFromPool(aiStrikerPrefab, position, Quaternion.identity)
            : gameplayItemPool.TryGetFromPool(playerStrikerPrefab, position, Quaternion.identity);

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
        gameplayItemPool.ReturnToPool(item);
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

    private void BindAbilityHud(PlayerSide side, StrikerBase striker)
    {
        var hud = GetAbilityHud(side);
        if (!hud) return;

        if (!striker)
        {
            hud.BindAbilityController(null);
            return;
        }

        if (striker.TryGetComponent(out PlayerAbilityController abilityController))
        {
            abilityController.SetPuckScaleController(GetPuckScaleController());
            abilityController.SetPaused(isAbilityPauseStateActive);
            hud.BindAbilityController(abilityController);
            return;
        }

        hud.BindAbilityController(null);
    }

    private void ClearAbilityHuds()
    {
        if (leftAbilityHud)
            leftAbilityHud.BindAbilityController(null);

        if (rightAbilityHud)
            rightAbilityHud.BindAbilityController(null);
    }

    private AbilityHudView GetAbilityHud(PlayerSide side)
    {
        return side == PlayerSide.Left ? leftAbilityHud : rightAbilityHud;
    }

    private IPuckScaleController GetPuckScaleController()
    {
        return puck ? puck.ScaleController : null;
    }

    private void ApplyAbilityPauseState(StrikerBase striker)
    {
        if (!striker) return;

        var abilityController = striker.AbilityController;
        abilityController?.SetPaused(isAbilityPauseStateActive);
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

        if (!leftAbilityHud)
            Debug.LogError($"{nameof(RoundController)} requires a left ability HUD reference.", this);

        if (!rightAbilityHud)
            Debug.LogError($"{nameof(RoundController)} requires a right ability HUD reference.", this);
    }
}
