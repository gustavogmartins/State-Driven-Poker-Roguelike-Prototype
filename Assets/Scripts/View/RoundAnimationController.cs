using DG.Tweening;
using Core;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using View;

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

public readonly struct ScoreCardAnimation {
    public ScoreCardAnimation(CardView cardView, int chipValue, int order) {
        CardView = cardView;
        ChipValue = chipValue;
        Order = order;
    }

    public CardView CardView { get; }
    public int ChipValue { get; }
    public int Order { get; }
}

public readonly struct ScoreJokerAnimation {
    public ScoreJokerAnimation(
        CardView cardView,
        JokerBonusType bonusType,
        int bonusValue,
        string popupText,
        int order) {
        CardView = cardView;
        BonusType = bonusType;
        BonusValue = bonusValue;
        PopupText = popupText;
        Order = order;
    }

    public CardView CardView { get; }
    public JokerBonusType BonusType { get; }
    public int BonusValue { get; }
    public string PopupText { get; }
    public int Order { get; }
}

public sealed class RoundAnimationController : MonoBehaviour {
    private static readonly Vector2 SelectedOffset = new(0f, 24f);
    private static readonly Vector3 SelectedScale = new(1.05f, 1.05f, 1f);
    private static readonly Vector3 SelectionPunchRotation = new(0f, 0f, 3f);
    private static readonly Vector3 ScorePunchScale = new(0.12f, 0.12f, 0f);
    private static readonly Vector3 ScorePunchRotation = new(0f, 0f, 2f);
    private const float PopupUpDirection = 1f;
    private const float PopupDownDirection = -1f;

    [SerializeField] private float selectionDuration = 0.16f;
    [SerializeField] private float selectionPunchDuration = 0.18f;
    [SerializeField] private float zoneChangeDuration = 0.34f;
    [SerializeField] private float zoneChangeStagger = 0.07f;
    [SerializeField] private ScorePopupView scorePopupPrefab;
    [SerializeField] private RectTransform scorePopupRoot;
    [SerializeField] private float scoreCardHighlightDuration = 0.14f;
    [SerializeField] private float scoreCardInterval = 0.28f;
    [SerializeField] private float scoreCardPunchDuration = 0.2f;
    [SerializeField] private float scoreCountDuration = 0.18f;
    [SerializeField] private float scorePopupDuration = 0.52f;
    [SerializeField] private float scorePopupStartOffsetY = 100f;
    [SerializeField] private float scorePopupFloatDistance = 54f;
    [SerializeField] private float scorePanelStepPause = 0.08f;

    private readonly Stack<ScorePopupView> _scorePopupPool = new();
    private int _activeBlockingAnimations;
    private TextMeshProUGUI _chipsText;
    private TextMeshProUGUI _multText;
    private TextMeshProUGUI _roundScoreText;

    public bool IsAnimating { get; private set; }

    public void ConfigureScoreTargets(
        TextMeshProUGUI chipsTarget,
        TextMeshProUGUI multTarget,
        TextMeshProUGUI roundScoreTarget,
        RectTransform popupRoot = null) {
        _chipsText = chipsTarget;
        _multText = multTarget;
        _roundScoreText = roundScoreTarget;
        scorePopupRoot = popupRoot != null ? popupRoot : scorePopupRoot;
    }

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

    public void AnimateScorePresentation(
        IReadOnlyList<ScoreCardAnimation> cardAnimations,
        IReadOnlyList<ScoreJokerAnimation> jokerAnimations,
        RoundViewModel viewModel,
        Action onComplete = null) {
        if (viewModel == null) {
            onComplete?.Invoke();
            return;
        }

        BeginBlockingAnimation();
        SetScorePresentationTexts(viewModel, finalValues: false);
        PrepareScoreTextTarget(_chipsText);
        PrepareScoreTextTarget(_multText);
        PrepareScoreTextTarget(_roundScoreText);

        bool completed = false;
        Sequence sequence = DOTween.Sequence();
        var orderedCards = SortScoreCardAnimations(cardAnimations);
        var orderedJokers = SortScoreJokerAnimations(jokerAnimations);
        float time = 0f;

        if (orderedCards.Count > 0 || orderedJokers.Count > 0) {
            for (int i = 0; i < orderedCards.Count; i++) {
                if (!TryPrepareScoreCard(orderedCards[i].CardView, out RectTransform visualRoot)) {
                    continue;
                }

                sequence.Insert(0f, visualRoot.DOAnchorPos(SelectedOffset, scoreCardHighlightDuration).SetEase(Ease.OutQuad));
                sequence.Insert(0f, visualRoot.DOScale(SelectedScale, scoreCardHighlightDuration).SetEase(Ease.OutQuad));
            }

            for (int i = 0; i < orderedJokers.Count; i++) {
                if (!TryPrepareScoreCard(orderedJokers[i].CardView, out RectTransform visualRoot)) {
                    continue;
                }

                sequence.Insert(0f, visualRoot.DOAnchorPos(SelectedOffset, scoreCardHighlightDuration).SetEase(Ease.OutQuad));
                sequence.Insert(0f, visualRoot.DOScale(SelectedScale, scoreCardHighlightDuration).SetEase(Ease.OutQuad));
            }

            time = scoreCardHighlightDuration + scorePanelStepPause;
        }

        int currentChips = viewModel.ScoreBaseChips;
        int currentMult = viewModel.ScoreBaseMult;
        int currentMultMultiplier = 1;
        float nextPostCardStepTime = time;
        for (int i = 0; i < orderedCards.Count; i++) {
            ScoreCardAnimation scoreCard = orderedCards[i];
            float stepTime = time;
            int fromChips = currentChips;
            int toChips = currentChips + scoreCard.ChipValue;
            currentChips = toChips;

            if (TryGetScoreCardVisualRoot(scoreCard.CardView, out RectTransform visualRoot)) {
                sequence.Insert(stepTime, visualRoot.DOPunchScale(ScorePunchScale, scoreCardPunchDuration, vibrato: 6, elasticity: 0.7f));
                sequence.Insert(stepTime, visualRoot.DOPunchRotation(ScorePunchRotation, scoreCardPunchDuration, vibrato: 6, elasticity: 0.55f));
                sequence.InsertCallback(stepTime, () => PlayScorePopup(visualRoot, scoreCard.ChipValue));
            }

            InsertIntTextTween(sequence, _chipsText, fromChips, toChips, stepTime + 0.04f, scoreCountDuration);
            InsertTextPunch(sequence, _chipsText, stepTime + 0.04f);
            nextPostCardStepTime = Mathf.Max(nextPostCardStepTime, stepTime + scorePopupDuration);
            time += scoreCardInterval;
        }

        time = Mathf.Max(time, nextPostCardStepTime + scorePanelStepPause);

        float nextPostJokerStepTime = time;
        for (int i = 0; i < orderedJokers.Count; i++) {
            ScoreJokerAnimation scoreJoker = orderedJokers[i];
            float stepTime = time;

            if (TryGetScoreCardVisualRoot(scoreJoker.CardView, out RectTransform visualRoot)) {
                sequence.Insert(stepTime, visualRoot.DOPunchScale(ScorePunchScale, scoreCardPunchDuration, vibrato: 6, elasticity: 0.7f));
                sequence.Insert(stepTime, visualRoot.DOPunchRotation(ScorePunchRotation, scoreCardPunchDuration, vibrato: 6, elasticity: 0.55f));
                sequence.InsertCallback(stepTime, () => PlayScorePopup(visualRoot, scoreJoker.PopupText, PopupDownDirection));
            }

            switch (scoreJoker.BonusType) {
                case JokerBonusType.Chips: {
                    int fromChips = currentChips;
                    int toChips = currentChips + scoreJoker.BonusValue;
                    InsertIntTextTween(sequence, _chipsText, fromChips, toChips, stepTime + 0.04f, scoreCountDuration);
                    InsertTextPunch(sequence, _chipsText, stepTime + 0.04f);
                    currentChips = toChips;
                    break;
                }
                case JokerBonusType.Mult: {
                    int fromMult = currentMult;
                    int toMult = currentMult + scoreJoker.BonusValue;
                    InsertIntTextTween(sequence, _multText, fromMult, toMult, stepTime + 0.04f, scoreCountDuration);
                    InsertTextPunch(sequence, _multText, stepTime + 0.04f);
                    currentMult = toMult;
                    break;
                }
                case JokerBonusType.XMult:
                    currentMultMultiplier *= scoreJoker.BonusValue;
                    int jokerDisplayMult = currentMult;
                    int jokerDisplayMultiplier = currentMultMultiplier;
                    sequence.InsertCallback(stepTime + 0.04f, () => SetText(_multText, FormatMultText(jokerDisplayMult, jokerDisplayMultiplier)));
                    InsertTextPunch(sequence, _multText, stepTime + 0.04f);
                    break;
            }

            nextPostJokerStepTime = Mathf.Max(nextPostJokerStepTime, stepTime + scorePopupDuration);
            time += scoreCardInterval;
        }

        time = Mathf.Max(time, nextPostJokerStepTime + scorePanelStepPause);

        if (currentChips != viewModel.ScoreTargetChips) {
            int fromChips = currentChips;
            int toChips = viewModel.ScoreTargetChips;
            InsertIntTextTween(sequence, _chipsText, fromChips, toChips, time, scoreCountDuration);
            InsertTextPunch(sequence, _chipsText, time);
            currentChips = toChips;
            time += scoreCardInterval;
        }

        if (currentMult != viewModel.ScoreTargetBaseMult) {
            InsertIntTextTween(sequence, _multText, currentMult, viewModel.ScoreTargetBaseMult, time, scoreCountDuration);
            InsertTextPunch(sequence, _multText, time);
            currentMult = viewModel.ScoreTargetBaseMult;
            time += scoreCardInterval;
        }

        if (currentMultMultiplier != viewModel.ScoreTargetMultMultiplier) {
            int targetBaseMult = currentMult;
            int targetMultMultiplier = viewModel.ScoreTargetMultMultiplier;
            sequence.InsertCallback(time, () => SetText(_multText, FormatMultText(targetBaseMult, targetMultMultiplier)));
            InsertTextPunch(sequence, _multText, time);
            time += scoreCardInterval;
        }

        InsertIntTextTween(sequence, _roundScoreText, viewModel.ScoreStartRoundScore, viewModel.ScoreTargetRoundScore, time + scorePanelStepPause, scoreCountDuration * 1.5f);
        InsertTextPunch(sequence, _roundScoreText, time + scorePanelStepPause);

        if (!sequence.active || sequence.Duration(includeLoops: false) <= 0f) {
            ResetScoreJokerVisuals(orderedJokers);
            SetScorePresentationTexts(viewModel, finalValues: true);
            EndBlockingAnimation();
            onComplete?.Invoke();
            return;
        }

        sequence.OnComplete(() => {
            completed = true;
            ResetScoreJokerVisuals(orderedJokers);
            SetScorePresentationTexts(viewModel, finalValues: true);
            EndBlockingAnimation();
            onComplete?.Invoke();
        });
        sequence.OnKill(() => {
            if (completed) {
                return;
            }

            ResetScoreJokerVisuals(orderedJokers);
            SetScorePresentationTexts(viewModel, finalValues: true);
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

    private static List<ScoreCardAnimation> SortScoreCardAnimations(IReadOnlyList<ScoreCardAnimation> cardAnimations) {
        var orderedCards = new List<ScoreCardAnimation>(cardAnimations ?? Array.Empty<ScoreCardAnimation>());
        orderedCards.Sort((left, right) => left.Order.CompareTo(right.Order));
        return orderedCards;
    }

    private static List<ScoreJokerAnimation> SortScoreJokerAnimations(IReadOnlyList<ScoreJokerAnimation> jokerAnimations) {
        var orderedJokers = new List<ScoreJokerAnimation>(jokerAnimations ?? Array.Empty<ScoreJokerAnimation>());
        orderedJokers.Sort((left, right) => left.Order.CompareTo(right.Order));
        return orderedJokers;
    }

    private static bool TryPrepareScoreCard(CardView cardView, out RectTransform visualRoot) {
        visualRoot = null;
        if (!TryGetScoreCardVisualRoot(cardView, out visualRoot)) {
            return false;
        }

        visualRoot.DOKill();
        visualRoot.anchoredPosition = Vector2.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;
        return true;
    }

    private static bool TryGetScoreCardVisualRoot(CardView cardView, out RectTransform visualRoot) {
        visualRoot = null;
        if (cardView == null) {
            return false;
        }

        visualRoot = cardView.VisualRoot;
        return visualRoot != null;
    }

    private static void ResetScoreJokerVisuals(IReadOnlyList<ScoreJokerAnimation> jokerAnimations) {
        if (jokerAnimations == null) {
            return;
        }

        for (int i = 0; i < jokerAnimations.Count; i++) {
            if (!TryGetScoreCardVisualRoot(jokerAnimations[i].CardView, out RectTransform visualRoot)) {
                continue;
            }

            visualRoot.DOKill();
            visualRoot.anchoredPosition = Vector2.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;
        }
    }

    private void SetScorePresentationTexts(RoundViewModel viewModel, bool finalValues) {
        if (finalValues) {
            SetText(_chipsText, viewModel.ScoreTargetChips.ToString());
            SetText(_multText, FormatMultText(viewModel.ScoreTargetBaseMult, viewModel.ScoreTargetMultMultiplier));
            SetText(_roundScoreText, viewModel.ScoreTargetRoundScore.ToString());
            return;
        }

        SetText(_chipsText, viewModel.ScoreBaseChips.ToString());
        SetText(_multText, viewModel.ScoreBaseMult.ToString());
        SetText(_roundScoreText, viewModel.ScoreStartRoundScore.ToString());
    }

    private static string FormatMultText(int baseMult, int multMultiplier) {
        return multMultiplier > 1
            ? $"{baseMult} x{multMultiplier}"
            : baseMult.ToString();
    }

    private static void SetText(TextMeshProUGUI target, string value) {
        if (target != null) {
            target.text = value;
        }
    }

    private static void InsertIntTextTween(
        Sequence sequence,
        TextMeshProUGUI target,
        int from,
        int to,
        float atPosition,
        float duration) {
        if (sequence == null || target == null) {
            return;
        }

        if (from == to) {
            sequence.InsertCallback(atPosition, () => target.text = to.ToString());
            return;
        }

        sequence.Insert(
            atPosition,
            DOVirtual.Int(from, to, duration, value => target.text = value.ToString()).SetEase(Ease.OutCubic));
    }

    private static void InsertTextPunch(Sequence sequence, TextMeshProUGUI target, float atPosition) {
        if (sequence == null || target == null) {
            return;
        }

        Transform targetTransform = target.transform;
        sequence.Insert(atPosition, targetTransform.DOPunchScale(ScorePunchScale, 0.18f, vibrato: 6, elasticity: 0.65f));
    }

    private static void PrepareScoreTextTarget(TextMeshProUGUI target) {
        if (target == null) {
            return;
        }

        target.transform.DOKill();
        target.transform.localScale = Vector3.one;
    }

    private void PlayScorePopup(RectTransform parent, int chipValue) {
        PlayScorePopup(parent, $"+{chipValue}", PopupUpDirection);
    }

    private void PlayScorePopup(RectTransform parent, string text, float verticalDirection) {
        if (parent == null) {
            return;
        }

        ScorePopupView popup = GetScorePopup(parent);
        if (popup == null) {
            return;
        }

        popup.Bind(text);
        float direction = verticalDirection < 0f ? PopupDownDirection : PopupUpDirection;
        float startOffsetY = scorePopupStartOffsetY * direction;
        float targetOffsetY = startOffsetY + scorePopupFloatDistance * direction;

        popup.RectTransform.anchoredPosition = new Vector2(0f, startOffsetY);
        popup.RectTransform.localRotation = Quaternion.identity;
        popup.RectTransform.localScale = Vector3.one;
        popup.CanvasGroup.alpha = 1f;
        popup.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();
        sequence.SetTarget(popup);
        sequence.Join(popup.RectTransform.DOAnchorPosY(targetOffsetY, scorePopupDuration).SetEase(Ease.OutCubic));
        sequence.Join(popup.CanvasGroup.DOFade(0f, scorePopupDuration).SetEase(Ease.InQuad));
        sequence.OnComplete(() => ReleaseScorePopup(popup));
    }

    private ScorePopupView GetScorePopup(RectTransform parent) {
        ScorePopupView popup = _scorePopupPool.Count > 0
            ? _scorePopupPool.Pop()
            : CreateScorePopup();

        if (popup == null) {
            return null;
        }

        popup.transform.SetParent(parent, false);
        popup.ResetView();
        return popup;
    }

    private ScorePopupView CreateScorePopup() {
        if (scorePopupPrefab != null) {
            return Instantiate(scorePopupPrefab);
        }

        var popupObject = new GameObject("ScorePopup", typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI), typeof(ScorePopupView));
        RectTransform rectTransform = (RectTransform)popupObject.transform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(96f, 40f);

        TextMeshProUGUI popupText = popupObject.GetComponent<TextMeshProUGUI>();
        popupText.alignment = TextAlignmentOptions.Center;
        popupText.fontSize = 36f;
        popupText.fontStyle = FontStyles.Bold;
        popupText.raycastTarget = false;
        return popupObject.GetComponent<ScorePopupView>();
    }

    private void ReleaseScorePopup(ScorePopupView popup) {
        if (popup == null) {
            return;
        }

        DOTween.Kill(popup);
        popup.RectTransform.DOKill();
        popup.CanvasGroup.DOKill();
        popup.ResetView();

        popup.gameObject.SetActive(false);
        popup.transform.SetParent(scorePopupRoot != null ? scorePopupRoot : transform, false);
        _scorePopupPool.Push(popup);
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
