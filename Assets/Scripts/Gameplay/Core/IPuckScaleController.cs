using DG.Tweening;

public interface IPuckScaleController
{
    bool CanToggleScale { get; }

    void ToggleScale(float downscaledScaleMultiplier, float animationDuration, Ease animationEase);
}
