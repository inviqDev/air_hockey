using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class StartGameMenu : MonoBehaviour
{
    [SerializeField] private GameObject choosingVsMenu;
    [SerializeField] private Button aiOpponentButton;
    [SerializeField] private Button secondPlayerButton;

    [Header("Choosing Menu Animation")]
    [SerializeField, Range(0.05f, 3f)] private float fadeInDuration = 1f;
    [SerializeField, Range(0.05f, 3f)] private float fadeOutDuration = 1f;

    public event Action AiOpponentSelected;
    public event Action SecondPlayerSelected;

    private CanvasGroup choosingVsMenuCanvasGroup;
    private Tween choosingVsMenuTween;

    private void Awake()
    {
        ValidateReferences();
        ResolveChoosingVsMenuCanvasGroup();
    }

    private void OnEnable()
    {
        if (aiOpponentButton != null)
        {
            aiOpponentButton.interactable = true;
            aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);
            aiOpponentButton.onClick.AddListener(SelectAiOpponent);
        }

        if (secondPlayerButton != null)
        {
            secondPlayerButton.interactable = true;
            secondPlayerButton.onClick.RemoveListener(SelectSecondPlayer);
            secondPlayerButton.onClick.AddListener(SelectSecondPlayer);
        }
    }

    private void OnDisable()
    {
        choosingVsMenuTween?.Kill();
        choosingVsMenuTween = null;

        if (aiOpponentButton != null)
        {
            aiOpponentButton.interactable = false;
            aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);
        }

        if (secondPlayerButton != null)
        {
            secondPlayerButton.interactable = false;
            secondPlayerButton.onClick.RemoveListener(SelectSecondPlayer);
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void ShowChoosingVsMenu()
    {
        if (choosingVsMenu == null)
        {
            return;
        }

        ResolveChoosingVsMenuCanvasGroup();
        choosingVsMenuTween?.Kill();
        choosingVsMenu.SetActive(true);

        if (choosingVsMenuCanvasGroup != null)
        {
            choosingVsMenuCanvasGroup.alpha = 0f;
            choosingVsMenuCanvasGroup.interactable = true;
            choosingVsMenuCanvasGroup.blocksRaycasts = true;

            choosingVsMenuTween = choosingVsMenuCanvasGroup
                .DOFade(1f, fadeInDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }
    }

    public void HideChoosingVsMenu()
    {
        if (choosingVsMenu == null)
        {
            return;
        }

        ResolveChoosingVsMenuCanvasGroup();
        choosingVsMenuTween?.Kill();

        if (choosingVsMenuCanvasGroup == null)
        {
            choosingVsMenu.SetActive(false);
            return;
        }

        choosingVsMenuCanvasGroup.interactable = false;
        choosingVsMenuCanvasGroup.blocksRaycasts = false;

        choosingVsMenuTween = choosingVsMenuCanvasGroup
            .DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .OnComplete(() => choosingVsMenu.SetActive(false));
    }

    private void SelectAiOpponent()
    {
        HideChoosingVsMenu();
        AiOpponentSelected?.Invoke();
    }

    private void SelectSecondPlayer()
    {
        HideChoosingVsMenu();
        SecondPlayerSelected?.Invoke();
    }

    private void ResolveChoosingVsMenuCanvasGroup()
    {
        if (choosingVsMenu == null)
        {
            choosingVsMenuCanvasGroup = null;
            return;
        }

        if (choosingVsMenuCanvasGroup == null)
        {
            choosingVsMenuCanvasGroup = choosingVsMenu.GetComponent<CanvasGroup>();
        }

        if (choosingVsMenuCanvasGroup == null)
        {
            choosingVsMenuCanvasGroup = choosingVsMenu.AddComponent<CanvasGroup>();
        }
    }

    private void ValidateReferences()
    {
        if (choosingVsMenu == null)
        {
            Debug.LogError($"{nameof(StartGameMenu)} requires a ChoosingVsMenu reference.", this);
        }

        if (aiOpponentButton == null)
        {
            Debug.LogError($"{nameof(StartGameMenu)} requires an AiOpponentButton reference.", this);
        }

        if (secondPlayerButton == null)
        {
            Debug.LogError($"{nameof(StartGameMenu)} requires a SecondPlayerButton reference.", this);
        }
    }
}
