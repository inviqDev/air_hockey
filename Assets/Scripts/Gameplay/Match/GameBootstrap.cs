using UnityEngine;

public sealed class GameBootstrap : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MatchManager matchManager;

    private void Awake()
    {
        ValidateReferences();
    }

    private void Start()
    {
        InitializeSceneStartup();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (!uiManager)
            Debug.LogError($"{nameof(GameBootstrap)} requires a UIManager reference.", this);

        if (!matchManager)
            Debug.LogError($"{nameof(GameBootstrap)} requires a MatchManager reference.", this);
    }

    private void InitializeSceneStartup()
    {
        if (!uiManager) return;
        if (!matchManager) return;

        uiManager.InitializeGameStart();
        matchManager.InitializeGameStart(uiManager);
    }
}
