using DG.Tweening;
using UnityEngine;

public sealed class RoundAnimationController : MonoBehaviour {
    private static readonly Vector2 SelectedOffset = new(0f, 24f);
    private static readonly Vector3 SelectedScale = new(1.05f, 1.05f, 1f);
    private static readonly Vector3 SelectionPunchRotation = new(0f, 0f, 3f);

    [SerializeField] private float selectionDuration = 0.16f;
    [SerializeField] private float selectionPunchDuration = 0.18f;

    public bool IsAnimating { get; private set; }

    public void AnimateCardSelection(CardView cardView, bool selected) {
        if (cardView == null) {
            return;
        }

        RectTransform visualRoot = cardView.VisualRoot;
        if (visualRoot == null) {
            return;
        }

        visualRoot.DOKill();

        Vector2 targetPosition = selected ? SelectedOffset : Vector2.zero;
        Vector3 targetScale = selected ? SelectedScale : Vector3.one;

        Sequence sequence = DOTween.Sequence();
        sequence.SetTarget(visualRoot);
        sequence.Join(visualRoot.DOAnchorPos(targetPosition, selectionDuration).SetEase(Ease.OutQuad));
        sequence.Join(visualRoot.DOScale(targetScale, selectionDuration).SetEase(Ease.OutQuad));

        if (selected) {
            sequence.Join(visualRoot.DOPunchRotation(SelectionPunchRotation, selectionPunchDuration, vibrato: 6, elasticity: 0.6f));
        } else {
            sequence.Join(visualRoot.DORotate(Vector3.zero, selectionDuration).SetEase(Ease.OutQuad));
        }
    }
}
