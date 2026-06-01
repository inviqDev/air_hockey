using System;
using DG.Tweening;
using UnityEngine;

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private OpponentSelectionView opponentSelection;
    [SerializeField] private SideSelectionView sideSelection;

    [Header("Animation")]
    [SerializeField, Range(0.05f, 3f)] private float fadeInDuration = 1f;
    [SerializeField, Range(0.05f, 3f)] private float fadeOutDuration = 1f;

    public event Action<MatchConfiguration> MatchConfigurationSelected;

    private CanvasGroup mainMenuCanvasGroup;
    private Tween mainMenuTween;
    private PlayerTwoControlType selectedPlayerTwoControlType;

    private void Awake()
    {
        ValidateReferences();
        ResolveMainMenuRoot();
        ShowOpponentSelection();
    }

    private void OnEnable()
    {
        if (opponentSelection)
        {
            opponentSelection.PlayerTwoControlTypeSelected += SelectMode;
        }

        if (sideSelection)
        {
            sideSelection.SideSelected += SelectSide;
            sideSelection.BackButtonClicked += ShowOpponentSelection;
        }
    }

    private void OnDisable()
    {
        mainMenuTween?.Kill();
        mainMenuTween = null;

        if (opponentSelection)
        {
            opponentSelection.PlayerTwoControlTypeSelected -= SelectMode;
        }

        if (sideSelection)
        {
            sideSelection.SideSelected -= SelectSide;
            sideSelection.BackButtonClicked -= ShowOpponentSelection;
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void Show()
    {
        if (!mainMenuRoot) return;

        ResolveMainMenuRoot();
        mainMenuTween?.Kill();
        mainMenuRoot.SetActive(true);

        if (!mainMenuCanvasGroup) return;

        mainMenuCanvasGroup.alpha = 0f;
        mainMenuCanvasGroup.interactable = true;
        mainMenuCanvasGroup.blocksRaycasts = true;
        ShowOpponentSelection();

        mainMenuTween = mainMenuCanvasGroup
            .DOFade(1f, fadeInDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    public void Hide()
    {
        if (!mainMenuRoot) return;

        ResolveMainMenuRoot();
        mainMenuTween?.Kill();

        if (!mainMenuCanvasGroup)
        {
            mainMenuRoot.SetActive(false);
            return;
        }

        mainMenuCanvasGroup.interactable = false;
        mainMenuCanvasGroup.blocksRaycasts = false;

        mainMenuTween = mainMenuCanvasGroup
            .DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .OnComplete(() => mainMenuRoot.SetActive(false));
    }

    private void SelectMode(PlayerTwoControlType playerTwoControlType)
    {
        selectedPlayerTwoControlType = playerTwoControlType;
        opponentSelection?.Hide();
        sideSelection?.Show();
    }

    private void ShowOpponentSelection()
    {
        sideSelection?.Hide();
        opponentSelection?.Show();
    }

    private void SelectSide(PlayerSide playerOneSide)
    {
        sideSelection?.Hide();
        opponentSelection?.Hide();
        Hide();

        MatchConfigurationSelected?.Invoke(new MatchConfiguration(selectedPlayerTwoControlType, playerOneSide));
    }

    private void ResolveMainMenuRoot()
    {
        if (!mainMenuRoot)
        {
            mainMenuRoot = gameObject;
        }

        if (!mainMenuCanvasGroup)
        {
            mainMenuCanvasGroup = mainMenuRoot.GetComponent<CanvasGroup>();
        }

        if (!mainMenuCanvasGroup)
        {
            mainMenuCanvasGroup = mainMenuRoot.AddComponent<CanvasGroup>();
        }
    }

    private void ValidateReferences()
    {
        if (!opponentSelection)
        {
            Debug.LogError($"{nameof(MainMenuController)} requires an opponent selection reference.", this);
        }

        if (!sideSelection)
        {
            Debug.LogError($"{nameof(MainMenuController)} requires a side selection reference.", this);
        }
    }
}
