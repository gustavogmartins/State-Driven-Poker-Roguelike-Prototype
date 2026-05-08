using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct CardZoneChangeAnimation {
    public CardZoneChangeAnimation(CardView cardView, CardZone from, CardZone to, int order) {
        CardView = cardView;
        From = from;
        To = to;
        Order = order;
    }

    public CardView CardView { get; }
    public CardZone From { get; }
    public CardZone To { get; }
    public int Order { get; }
}

public sealed class RoundAnimationController : MonoBehaviour {
    private static readonly Vector2 SelectedOffset = new(0f, 24f);
    private static readonly Vector3 SelectedScale = new(1.05f, 1.05f, 1f);
    private static readonly Vector3 SelectionPunchRotation = new(0f, 0f, 3f);

    [SerializeField] private float selectionDuration = 0.16f;
    [SerializeField] private float selectionPunchDuration = 0.18f;
    [SerializeField] private float zoneChangeDuration = 0.34f;
    [SerializeField] private float zoneChangeStagger = 0.07f;

    private int _activeBlockingAnimations;

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

    public void AnimateCardZoneChange(CardView cardView, CardZone from, CardZone to, Action onComplete = null) {
        AnimateCardZoneChangeSequence(
            new[] { new CardZoneChangeAnimation(cardView, from, to, 0) },
            onComplete);
    }

    public void AnimateCardZoneChangeSequence(IReadOnlyList<CardZoneChangeAnimation> moves, Action onComplete = null) {
        if (moves == null || moves.Count == 0) {
            onComplete?.Invoke();
            return;
        }

        BeginBlockingAnimation();

        bool completed = false;
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < moves.Count; i++) {
            CardZoneChangeAnimation move = moves[i];
            if (!TryPrepareZoneChange(move.CardView, out RectTransform cardTransform)) {
                continue;
            }

            float startTime = i * zoneChangeStagger;
            sequence.Insert(startTime, cardTransform.DOAnchorPos(Vector2.zero, zoneChangeDuration).SetEase(Ease.InOutCubic));
            sequence.Insert(startTime, cardTransform.DOScale(Vector3.one, zoneChangeDuration).SetEase(Ease.OutQuad));
        }

        if (!sequence.active || sequence.Duration(includeLoops: false) <= 0f) {
            EndBlockingAnimation();
            onComplete?.Invoke();
            return;
        }

        sequence.OnComplete(() => {
            completed = true;
            CompleteZoneChangeMoves(moves);
            EndBlockingAnimation();
            onComplete?.Invoke();
        });
        sequence.OnKill(() => {
            if (completed) {
                return;
            }

            CompleteZoneChangeMoves(moves);
            EndBlockingAnimation();
        });
    }

    private static bool TryPrepareZoneChange(CardView cardView, out RectTransform cardTransform) {
        cardTransform = null;
        if (cardView == null) {
            return false;
        }

        RectTransform visualRoot = cardView.VisualRoot;
        cardTransform = cardView.RectTransform;
        if (cardTransform == null || visualRoot == null) {
            return false;
        }

        cardTransform.DOKill();
        visualRoot.DOKill();
        visualRoot.anchoredPosition = Vector2.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        return true;
    }

    private static void CompleteZoneChangeMoves(IReadOnlyList<CardZoneChangeAnimation> moves) {
        for (int i = 0; i < moves.Count; i++) {
            CardView cardView = moves[i].CardView;
            if (cardView == null) {
                continue;
            }

            RectTransform cardTransform = cardView.RectTransform;
            if (cardTransform == null) {
                continue;
            }

            cardTransform.anchoredPosition = Vector2.zero;
            cardTransform.localRotation = Quaternion.identity;
            cardTransform.localScale = Vector3.one;
        }
    }

    private void BeginBlockingAnimation() {
        _activeBlockingAnimations++;
        IsAnimating = true;
    }

    private void EndBlockingAnimation() {
        _activeBlockingAnimations = Mathf.Max(0, _activeBlockingAnimations - 1);
        IsAnimating = _activeBlockingAnimations > 0;
    }
}
