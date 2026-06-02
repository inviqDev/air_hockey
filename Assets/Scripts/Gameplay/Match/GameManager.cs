using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MatchManager matchManager;

    public UIManager UIManager => uiManager;
    public MatchManager MatchManager => matchManager;

    private void Awake()
    {
        ValidateReferences();
    }

    private void Start()
    {
        InitializeGameStartFlow();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (!uiManager)
            Debug.LogError($"{nameof(GameManager)} requires a UIManager reference.", this);

        if (!matchManager)
            Debug.LogError($"{nameof(GameManager)} requires a MatchManager reference.", this);
    }

    private void InitializeGameStartFlow()
    {
        if (!uiManager) return;
        if (!matchManager) return;

        uiManager.InitializeGameStart();
        matchManager.InitializeGameStart(uiManager);
    }
}
