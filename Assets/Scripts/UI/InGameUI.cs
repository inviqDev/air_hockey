using DG.Tweening;
using UnityEngine;

public sealed class InGameUI : MonoBehaviour
{
        [Header("References")]
        [SerializeField] private GameObject inGameUIRoot;

        [Header("Appear Animation")]
        [SerializeField, Min(0f)] private float appearDelaySeconds = 0.5f;
        [SerializeField, Min(0.01f)] private float fadeInDuration = 0.85f;
        [SerializeField] private Ease fadeEase = Ease.InBack;

        [Header("Scale Animation")]
        [SerializeField] private bool animateScale = true;
        [SerializeField] private Vector3 hiddenScale = new(0.65f, 0.65f, 1f);
        [SerializeField] private Vector3 visibleScale = Vector3.one;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        private CanvasGroup canvasGroup;
        private Tween appearTween;

        private bool firstAppear = true;
        
        private void Initialize()
        {
            ValidateReferences();
            ResolveRoot();
            HideImmediately();
        }

        private void OnDisable()
        {
            StopAnimation();
        }

        private void OnValidate()
        {
            ValidateReferences();
        }

        [ContextMenu("Show")]
        public void Show()
        {
            if (!inGameUIRoot) return;
            
            if (firstAppear)
            {
                Initialize();
                firstAppear = false;
            }

            ResolveRoot();
            StopAnimation();

            inGameUIRoot.SetActive(true);
            
            if (!canvasGroup) return;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (animateScale)
            {
                inGameUIRoot.transform.localScale = hiddenScale;
            }

            var sequence = DOTween.Sequence().SetUpdate(true);
            sequence.AppendInterval(appearDelaySeconds);
            sequence.Append(canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeEase));

            if (animateScale)
            {
                sequence.Join(inGameUIRoot.transform.DOScale(visibleScale, fadeInDuration).SetEase(scaleEase));
            }

            appearTween = sequence.OnComplete(() => appearTween = null);
        }

        [ContextMenu("Hide Immediately")]
        public void HideImmediately()
        {
            if (!inGameUIRoot) return;

            ResolveRoot();
            StopAnimation();

            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (animateScale)
            {
                inGameUIRoot.transform.localScale = hiddenScale;
            }

            inGameUIRoot.SetActive(false);
        }

        private void ResolveRoot()
        {
            if (!inGameUIRoot)
            {
                inGameUIRoot = gameObject;
            }

            if (canvasGroup) return;
            
            if (inGameUIRoot.TryGetComponent<CanvasGroup>(out var canvas))
            {
                canvasGroup = canvas;
                return;
            }

            canvasGroup = inGameUIRoot.AddComponent<CanvasGroup>();
        }

        private void StopAnimation()
        {
            appearTween?.Kill();
            appearTween = null;
        }

        private void ValidateReferences()
        {
            if (inGameUIRoot) return;
            inGameUIRoot = gameObject;
        }
}
