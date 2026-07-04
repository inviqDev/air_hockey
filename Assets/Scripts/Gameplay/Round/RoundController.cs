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
    [SerializeField] private ParticipantHudView leftParticipantHud;
    [SerializeField] private ParticipantHudView rightParticipantHud;

    private Puck puck;
    private StrikerBase leftStriker;
    private StrikerBase rightStriker;
    private bool isAbilityPauseStateActive;
    private PlayerInputMode currentInputMode = PlayerInputMode.Disabled;

    private readonly Pool gameplayItemPool = new();

    public bool HasAllRoundItemsActive =>
        IsRoundItemActive(puck) &&
        IsRoundItemActive(leftStriker) &&
        IsRoundItemActive(rightStriker);

    public bool ActivateRoundItems(MatchConfiguration configuration)
    {
        ValidateTemporaryTwoSideConfiguration(configuration);

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
        var participantSetup = GetParticipantForSlot(configuration, TemporaryTwoSideArena.LeftSlot);
        var spawnPosition = GetPosition(leftStrikerSpawnPoint);
        leftStriker = ActivateStrikerFromPool(participantSetup, PlayerSide.Left, spawnPosition);
        BindAbilityHud(PlayerSide.Left, leftStriker);

        participantSetup = GetParticipantForSlot(configuration, TemporaryTwoSideArena.RightSlot);
        spawnPosition = GetPosition(rightStrikerSpawnPoint);
        rightStriker = ActivateStrikerFromPool(participantSetup, PlayerSide.Right, spawnPosition);
        BindAbilityHud(PlayerSide.Right, rightStriker);

        ApplyPlayerInputContextToActiveReaders();
    }

    private static MatchParticipantSetup GetParticipantForSlot(MatchConfiguration configuration, ArenaSlotId slotId)
    {
        var participantId = configuration.SlotAssignments.GetParticipantForSlot(slotId);
        var participantSetup = configuration.Roster.GetParticipantSetupById(participantId);

        return participantSetup;
    }

    private StrikerBase ActivateStrikerFromPool(MatchParticipantSetup participantSetup, PlayerSide side, Vector2 position)
    {
        StrikerSetupContext setupContext;
        StrikerBase striker;

        if (participantSetup.IsHuman)
        {
            setupContext = new StrikerSetupContext(side, puck, participantSetup.GetRequiredHumanControlScheme());
            striker = gameplayItemPool.TryGetFromPool(playerStrikerPrefab, position, Quaternion.identity);
        }
        else if (participantSetup.IsAi)
        {
            setupContext = new StrikerSetupContext(side, puck);
            striker = gameplayItemPool.TryGetFromPool(aiStrikerPrefab, position, Quaternion.identity);
        }
        else
        {
            throw new System.InvalidOperationException(
                $"Unsupported participant control source {participantSetup.ControlSource}.");
        }

        if (striker)
            striker.Initialize(setupContext, turnController);

        return striker;
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
        ReturnRoundItemsToPool(resetAbilitiesForFullMatch: false);
    }

    public void ReturnRoundItemsToPoolForFullMatch()
    {
        ReturnRoundItemsToPool(resetAbilitiesForFullMatch: true);
    }

    private void ReturnRoundItemsToPool(bool resetAbilitiesForFullMatch)
    {
        ReturnStrikerToPool(leftStriker, resetAbilitiesForFullMatch);
        ReturnStrikerToPool(rightStriker, resetAbilitiesForFullMatch);
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

    public void ApplyPlayerInputMode(PlayerInputMode inputMode)
    {
        currentInputMode = inputMode;
        ApplyPlayerInputContextToActiveReaders();
    }

    public PlayerAbilityController GetAbilityController(ArenaSlotId slotId)
    {
        var side = GetSideForSlot(slotId);
        var striker = side == PlayerSide.Left ? leftStriker : rightStriker;
        return striker ? striker.AbilityController : null;
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
        InitializeAbilityHuds();
        ReturnRoundItemsToPool();
    }

    private static void ValidateTemporaryTwoSideConfiguration(MatchConfiguration configuration)
    {
        if (configuration == null)
            throw new System.ArgumentNullException(nameof(configuration));

        if (configuration.ArenaId != TemporaryTwoSideArena.ArenaId)
        {
            throw new System.InvalidOperationException(
                $"{nameof(RoundController)} only supports arena {TemporaryTwoSideArena.ArenaId}, but received {configuration.ArenaId}.");
        }
    }

    private static PlayerSide GetSideForSlot(ArenaSlotId slotId)
    {
        if (slotId == TemporaryTwoSideArena.LeftSlot)
            return PlayerSide.Left;

        if (slotId == TemporaryTwoSideArena.RightSlot)
            return PlayerSide.Right;

        throw new System.ArgumentOutOfRangeException(
            nameof(slotId), slotId, $"{nameof(RoundController)} only supports the temporary two-side arena slots.");
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

    private void ReturnStrikerToPool(StrikerBase striker, bool resetAbilitiesForFullMatch)
    {
        if (!striker) return;

        if (resetAbilitiesForFullMatch)
        {
            var abilityController = striker.AbilityController;
            if (abilityController)
                abilityController.ResetForFullMatch();
        }

        gameplayItemPool.ReturnToPool(striker);
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
        if (leftParticipantHud)
            leftParticipantHud.BindAbilityController(null);

        if (rightParticipantHud)
            rightParticipantHud.BindAbilityController(null);
    }

    private ParticipantHudView GetAbilityHud(PlayerSide side)
    {
        return side == PlayerSide.Left ? leftParticipantHud : rightParticipantHud;
    }

    private void InitializeAbilityHuds()
    {
        if (leftParticipantHud)
            leftParticipantHud.Initialize();

        if (rightParticipantHud)
            rightParticipantHud.Initialize();
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

    private void ApplyPlayerInputContextToActiveReaders()
    {
        var leftReader = GetPlayerInputReader(leftStriker);
        var rightReader = GetPlayerInputReader(rightStriker);

        if (leftReader)
            leftReader.SetInputMode(currentInputMode);

        if (rightReader && rightReader != leftReader)
            rightReader.SetInputMode(currentInputMode);
    }

    private static PlayerInputReader GetPlayerInputReader(StrikerBase striker)
    {
        if (!striker) return null;

        var abilityController = striker.AbilityController;
        return abilityController ? abilityController.InputReader : null;
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

        if (!leftParticipantHud)
            Debug.LogError($"{nameof(RoundController)} requires a left ability HUD reference.", this);

        if (!rightParticipantHud)
            Debug.LogError($"{nameof(RoundController)} requires a right ability HUD reference.", this);
    }
}
