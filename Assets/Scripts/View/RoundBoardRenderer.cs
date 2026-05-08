using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using View;

public sealed class RoundBoardRenderer : MonoBehaviour {
    [SerializeField] private RectTransform handArea;
    [SerializeField] private RectTransform playedHandArea;
    [SerializeField] private RectTransform discardedCardsArea;
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private RoundAnimationController animationController;
    [SerializeField] private CardViewPool cardViewPool;
    [SerializeField] private int handSlotCount = 8;
    [SerializeField] private int playedCardSlotCount = 5;
    [SerializeField] private float scoringPresentationDelay = 0.12f;

    private readonly Dictionary<int, CardView> _cardViewsById = new();
    private readonly List<RectTransform> _handSlots = new();
    private readonly List<RectTransform> _playedCardSlots = new();
    private readonly List<int> _pendingDiscardCardIds = new();
    private RoundViewModel _previousViewModel;
    private RoundViewModel _pendingScoringViewModel;
    private bool _scoringPresentationPending;
    private bool _scoringPresentationRunning;
    private bool _discardPresentationPending;
    private Coroutine _scoringPresentationCoroutine;

    public event Action<int> CardSelected;
    public event Action ScoringPresentationFinished;
    public event Action DiscardPresentationFinished;

    public void Configure(
        CardView prefab,
        RectTransform handCardsArea,
        RectTransform playedCardsArea,
        RectTransform discardedCardsTargetArea,
        RoundAnimationController controller,
        CardViewPool pool,
        int configuredHandSlotCount,
        int configuredPlayedCardSlotCount) {
        cardPrefab = prefab != null ? prefab : cardPrefab;
        handArea = handCardsArea != null ? handCardsArea : handArea;
        playedHandArea = playedCardsArea != null ? playedCardsArea : playedHandArea;
        discardedCardsArea = discardedCardsTargetArea != null ? discardedCardsTargetArea : discardedCardsArea;
        animationController = controller != null ? controller : animationController;
        cardViewPool = pool != null ? pool : cardViewPool;
        handSlotCount = Math.Max(0, configuredHandSlotCount);
        playedCardSlotCount = Math.Max(0, configuredPlayedCardSlotCount);
        cardViewPool?.Configure(cardPrefab);
    }

    public void Render(RoundViewModel viewModel) {
        if (viewModel == null) {
            return;
        }

        EnsureSlots(handArea, _handSlots, Math.Max(handSlotCount, viewModel.HandCards.Count), "HandSlot");
        EnsureSlots(playedHandArea, _playedCardSlots, Math.Max(playedCardSlotCount, viewModel.PlayedCards.Count), "PlayedCardSlot");
        bool enteredScoring = _previousViewModel != null &&
            _previousViewModel.Phase != RoundPhase.Scoring &&
            viewModel.Phase == RoundPhase.Scoring;
        bool enteredDiscarding = _previousViewModel != null &&
            _previousViewModel.Phase != RoundPhase.Discarding &&
            viewModel.Phase == RoundPhase.Discarding;

        if (enteredScoring) {
            BeginScoringPresentationTracking(viewModel);
        }

        if (enteredDiscarding) {
            BeginDiscardPresentationTracking(viewModel);
        }

        var zoneChangeAnimations = new List<CardZoneChangeAnimation>();
        ApplyCards(viewModel, zoneChangeAnimations);

        if (_previousViewModel != null) {
            DetectSelectionChanges(_previousViewModel, viewModel);
        }

        Action onZoneChangeComplete = null;
        if (enteredScoring) {
            onZoneChangeComplete = ScheduleScoringPresentationFinished;
        } else if (enteredDiscarding) {
            onZoneChangeComplete = FinishDiscardPresentation;
        }

        PlayZoneChangeAnimations(zoneChangeAnimations, onZoneChangeComplete);

        if ((enteredScoring || enteredDiscarding) && zoneChangeAnimations.Count == 0) {
            onZoneChangeComplete?.Invoke();
        }

        _previousViewModel = viewModel;
    }

    private void ApplyCards(RoundViewModel viewModel, List<CardZoneChangeAnimation> zoneChangeAnimations) {
        var visibleCardIds = new HashSet<int>();

        if (playedHandArea != null) {
            playedHandArea.gameObject.SetActive(viewModel.PlayedCards.Count > 0);
        }

        RebuildCardAreaLayouts();

        for (int i = 0; i < viewModel.HandCards.Count; i++) {
            CardViewModel cardViewModel = viewModel.HandCards[i];
            if (i >= _handSlots.Count) {
                continue;
            }

            visibleCardIds.Add(cardViewModel.CardId);
            BindCardToSlot(cardViewModel, _handSlots[i], zoneChangeAnimations);
        }

        for (int i = 0; i < viewModel.PlayedCards.Count; i++) {
            CardViewModel cardViewModel = viewModel.PlayedCards[i];
            if (i >= _playedCardSlots.Count) {
                continue;
            }

            visibleCardIds.Add(cardViewModel.CardId);
            BindCardToSlot(cardViewModel, _playedCardSlots[i], zoneChangeAnimations);
        }

        for (int i = 0; i < viewModel.DiscardedCards.Count; i++) {
            CardViewModel cardViewModel = viewModel.DiscardedCards[i];
            if (discardedCardsArea == null) {
                continue;
            }

            visibleCardIds.Add(cardViewModel.CardId);
            BindCardToSlot(cardViewModel, discardedCardsArea, zoneChangeAnimations);
        }

        ReleaseCardsNotIn(visibleCardIds);
        RebuildCardAreaLayouts();
    }

    private void BindCardToSlot(CardViewModel cardViewModel, RectTransform slot, List<CardZoneChangeAnimation> zoneChangeAnimations) {
        CardView cardView = GetOrCreateCardView(cardViewModel.CardId, slot);
        if (cardView == null) {
            return;
        }

        CardViewModel previousCard = _previousViewModel != null
            ? FindCard(_previousViewModel, cardViewModel.CardId)
            : null;
        bool shouldAnimateZoneChange = previousCard != null && previousCard.Zone != cardViewModel.Zone;
        Vector3 previousWorldPosition = cardView.RectTransform.position;

        ParentCardToSlot(cardView, slot);
        cardView.OnCardSelected -= HandleCardSelected;
        cardView.OnCardSelected += HandleCardSelected;
        cardView.Bind(cardViewModel);

        if (!shouldAnimateZoneChange) {
            return;
        }

        cardView.RectTransform.position = previousWorldPosition;
        int order = previousCard.Index >= 0 ? previousCard.Index : zoneChangeAnimations.Count;
        zoneChangeAnimations.Add(new CardZoneChangeAnimation(cardView, previousCard.Zone, cardViewModel.Zone, order));
    }

    private CardView GetOrCreateCardView(int cardId, RectTransform parent) {
        if (cardId <= 0 || parent == null) {
            return null;
        }

        if (_cardViewsById.TryGetValue(cardId, out CardView existing) && existing != null) {
            return existing;
        }

        if (cardPrefab == null) {
            return null;
        }

        CardView created = cardViewPool != null
            ? cardViewPool.Get(parent)
            : Instantiate(cardPrefab, parent);

        if (created == null) {
            return null;
        }

        _cardViewsById[cardId] = created;
        return created;
    }

    private void DetectSelectionChanges(RoundViewModel previous, RoundViewModel current) {
        foreach (CardViewModel currentCard in current.GameplayCards) {
            CardViewModel previousCard = FindCard(previous, currentCard.CardId);
            if (previousCard == null || previousCard.IsSelected == currentCard.IsSelected) {
                continue;
            }

            if (previousCard.Zone != currentCard.Zone) {
                continue;
            }

            if (_cardViewsById.TryGetValue(currentCard.CardId, out CardView cardView)) {
                animationController?.AnimateCardSelection(cardView, currentCard.IsSelected);
            }
        }
    }

    private static CardViewModel FindCard(RoundViewModel viewModel, int cardId) {
        foreach (CardViewModel card in viewModel.GameplayCards) {
            if (card.CardId == cardId) {
                return card;
            }
        }

        return null;
    }

    private void ReleaseCardsNotIn(HashSet<int> visibleCardIds) {
        var cardsToRelease = new List<int>();

        foreach (var pair in _cardViewsById) {
            if (!visibleCardIds.Contains(pair.Key)) {
                cardsToRelease.Add(pair.Key);
            }
        }

        for (int i = 0; i < cardsToRelease.Count; i++) {
            int cardId = cardsToRelease[i];
            CardView cardView = _cardViewsById[cardId];
            if (cardView != null) {
                cardView.OnCardSelected -= HandleCardSelected;
            }

            _cardViewsById.Remove(cardId);

            if (cardViewPool != null) {
                cardViewPool.Release(cardView);
            } else if (cardView != null) {
                Destroy(cardView.gameObject);
            }
        }
    }

    private void HandleCardSelected(int index) {
        CardSelected?.Invoke(index);
    }

    private void BeginScoringPresentationTracking(RoundViewModel viewModel) {
        _scoringPresentationPending = true;
        _scoringPresentationRunning = false;
        _pendingScoringViewModel = viewModel;

        if (_scoringPresentationCoroutine != null) {
            StopCoroutine(_scoringPresentationCoroutine);
            _scoringPresentationCoroutine = null;
        }
    }

    private void BeginDiscardPresentationTracking(RoundViewModel viewModel) {
        _discardPresentationPending = true;
        _pendingDiscardCardIds.Clear();

        foreach (CardViewModel card in viewModel.DiscardedCards) {
            _pendingDiscardCardIds.Add(card.CardId);
        }
    }

    private void ScheduleScoringPresentationFinished() {
        if (!_scoringPresentationPending || _scoringPresentationRunning || _scoringPresentationCoroutine != null) {
            return;
        }

        if (animationController == null) {
            _scoringPresentationCoroutine = StartCoroutine(NotifyScoringPresentationFinished());
            return;
        }

        _scoringPresentationRunning = true;
        animationController.AnimateScorePresentation(
            BuildScoreCardAnimations(_pendingScoringViewModel),
            _pendingScoringViewModel,
            CompleteScoringPresentation);
    }

    private IEnumerator NotifyScoringPresentationFinished() {
        if (scoringPresentationDelay > 0f) {
            yield return new WaitForSeconds(scoringPresentationDelay);
        } else {
            yield return null;
        }

        _scoringPresentationCoroutine = null;
        CompleteScoringPresentation();
    }

    private void CompleteScoringPresentation() {
        if (!_scoringPresentationPending) {
            return;
        }

        _scoringPresentationRunning = false;
        _scoringPresentationPending = false;
        _pendingScoringViewModel = null;
        ScoringPresentationFinished?.Invoke();
    }

    private void FinishDiscardPresentation() {
        if (!_discardPresentationPending) {
            return;
        }

        ReleaseCardsById(_pendingDiscardCardIds);
        _pendingDiscardCardIds.Clear();
        _discardPresentationPending = false;
        DiscardPresentationFinished?.Invoke();
    }

    private void RebuildCardAreaLayouts() {
        if (handArea != null) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(handArea);
        }

        if (playedHandArea != null) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(playedHandArea);
        }
    }

    private void PlayZoneChangeAnimations(List<CardZoneChangeAnimation> zoneChangeAnimations, Action onComplete) {
        if (zoneChangeAnimations == null || zoneChangeAnimations.Count == 0) {
            return;
        }

        zoneChangeAnimations.Sort((left, right) => left.Order.CompareTo(right.Order));

        if (animationController != null) {
            animationController.AnimateCardZoneChangeSequence(zoneChangeAnimations, onComplete);
            return;
        }

        CompleteZoneChangeAnimationsWithoutController(zoneChangeAnimations);
        onComplete?.Invoke();
    }

    private void ReleaseCardsById(IReadOnlyList<int> cardIds) {
        for (int i = 0; i < cardIds.Count; i++) {
            int cardId = cardIds[i];
            if (!_cardViewsById.TryGetValue(cardId, out CardView cardView)) {
                continue;
            }

            if (cardView != null) {
                cardView.OnCardSelected -= HandleCardSelected;
            }

            _cardViewsById.Remove(cardId);

            if (cardViewPool != null) {
                cardViewPool.Release(cardView);
            } else if (cardView != null) {
                Destroy(cardView.gameObject);
            }
        }
    }

    private IReadOnlyList<ScoreCardAnimation> BuildScoreCardAnimations(RoundViewModel viewModel) {
        var animations = new List<ScoreCardAnimation>();
        if (viewModel == null) {
            return animations;
        }

        for (int i = 0; i < viewModel.PlayedCards.Count; i++) {
            CardViewModel card = viewModel.PlayedCards[i];
            if (!card.IsScoringCard || card.ScoringChipValue <= 0) {
                continue;
            }

            if (!_cardViewsById.TryGetValue(card.CardId, out CardView cardView)) {
                continue;
            }

            animations.Add(new ScoreCardAnimation(cardView, card.ScoringChipValue, i));
        }

        return animations;
    }

    private static void CompleteZoneChangeAnimationsWithoutController(IReadOnlyList<CardZoneChangeAnimation> zoneChangeAnimations) {
        for (int i = 0; i < zoneChangeAnimations.Count; i++) {
            CardView cardView = zoneChangeAnimations[i].CardView;
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

    private void EnsureSlots(RectTransform area, List<RectTransform> slots, int slotCount, string slotNamePrefix) {
        if (area == null || cardPrefab == null) {
            return;
        }

        for (int i = slots.Count - 1; i >= 0; i--) {
            if (slots[i] == null || slots[i].parent != area) {
                slots.RemoveAt(i);
            }
        }

        RectTransform prefabTransform = cardPrefab.transform as RectTransform;
        Vector2 slotSize = prefabTransform != null ? prefabTransform.sizeDelta : new Vector2(165f, 230f);

        while (slots.Count < slotCount) {
            var slotObject = new GameObject($"{slotNamePrefix}{slots.Count + 1}", typeof(RectTransform), typeof(LayoutElement));
            RectTransform slot = (RectTransform)slotObject.transform;
            slot.SetParent(area, false);
            slot.anchorMin = new Vector2(0.5f, 0.5f);
            slot.anchorMax = new Vector2(0.5f, 0.5f);
            slot.pivot = new Vector2(0.5f, 0.5f);
            slot.sizeDelta = slotSize;

            LayoutElement layoutElement = slotObject.GetComponent<LayoutElement>();
            layoutElement.minWidth = slotSize.x;
            layoutElement.minHeight = slotSize.y;
            layoutElement.preferredWidth = slotSize.x;
            layoutElement.preferredHeight = slotSize.y;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;

            slots.Add(slot);
        }

        for (int i = 0; i < slots.Count; i++) {
            slots[i].gameObject.SetActive(i < slotCount);
            slots[i].SetSiblingIndex(i);
        }
    }

    private void ParentCardToSlot(CardView cardView, RectTransform slot) {
        if (cardView == null || slot == null) {
            return;
        }

        RectTransform cardTransform = cardView.RectTransform;
        cardTransform.SetParent(slot, false);
        cardTransform.anchorMin = new Vector2(0.5f, 0.5f);
        cardTransform.anchorMax = new Vector2(0.5f, 0.5f);
        cardTransform.pivot = new Vector2(0.5f, 0.5f);
        cardTransform.anchoredPosition = Vector2.zero;
        cardTransform.localRotation = Quaternion.identity;
        cardTransform.localScale = Vector3.one;

        if (cardPrefab != null && cardPrefab.transform is RectTransform prefabTransform) {
            cardTransform.sizeDelta = prefabTransform.sizeDelta;
        }
    }
}
