using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using View;

public sealed class RoundBoardRenderer : MonoBehaviour {
    [SerializeField] private RectTransform handArea;
    [SerializeField] private RectTransform playedHandArea;
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private RoundAnimationController animationController;
    [SerializeField] private CardViewPool cardViewPool;
    [SerializeField] private int handSlotCount = 8;
    [SerializeField] private int playedCardSlotCount = 5;

    private readonly Dictionary<int, CardView> _cardViewsById = new();
    private readonly List<RectTransform> _handSlots = new();
    private readonly List<RectTransform> _playedCardSlots = new();
    private RoundViewModel _previousViewModel;

    public event Action<int> CardSelected;

    public void Configure(
        CardView prefab,
        RectTransform handCardsArea,
        RectTransform playedCardsArea,
        RoundAnimationController controller,
        CardViewPool pool,
        int configuredHandSlotCount,
        int configuredPlayedCardSlotCount) {
        cardPrefab = prefab != null ? prefab : cardPrefab;
        handArea = handCardsArea != null ? handCardsArea : handArea;
        playedHandArea = playedCardsArea != null ? playedCardsArea : playedHandArea;
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
        ApplyCards(viewModel);

        if (_previousViewModel != null) {
            DetectSelectionChanges(_previousViewModel, viewModel);
        }

        _previousViewModel = viewModel;
    }

    private void ApplyCards(RoundViewModel viewModel) {
        var visibleCardIds = new HashSet<int>();

        for (int i = 0; i < viewModel.HandCards.Count; i++) {
            CardViewModel cardViewModel = viewModel.HandCards[i];
            if (i >= _handSlots.Count) {
                continue;
            }

            visibleCardIds.Add(cardViewModel.CardId);
            BindCardToSlot(cardViewModel, _handSlots[i]);
        }

        if (playedHandArea != null) {
            playedHandArea.gameObject.SetActive(viewModel.PlayedCards.Count > 0);
        }

        for (int i = 0; i < viewModel.PlayedCards.Count; i++) {
            CardViewModel cardViewModel = viewModel.PlayedCards[i];
            if (i >= _playedCardSlots.Count) {
                continue;
            }

            visibleCardIds.Add(cardViewModel.CardId);
            BindCardToSlot(cardViewModel, _playedCardSlots[i]);
        }

        ReleaseCardsNotIn(visibleCardIds);
        if (handArea != null) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(handArea);
        }

        if (playedHandArea != null) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(playedHandArea);
        }
    }

    private void BindCardToSlot(CardViewModel cardViewModel, RectTransform slot) {
        CardView cardView = GetOrCreateCardView(cardViewModel.CardId, slot);
        if (cardView == null) {
            return;
        }

        ParentCardToSlot(cardView, slot);
        cardView.OnCardSelected -= HandleCardSelected;
        cardView.OnCardSelected += HandleCardSelected;
        cardView.Bind(cardViewModel);
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
