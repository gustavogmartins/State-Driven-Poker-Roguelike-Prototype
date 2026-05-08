using System;
using System.Collections.Generic;
using System.Linq;

namespace Core {
    public static class RoundReducer {
        private const int MaxSelectableCards = 5;

        public static RoundState Reduce(RoundState state, GameAction action) {
            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }

            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            return action switch {
                ToggleCardSelectionAction toggle => ToggleCardSelection(state, toggle.Index),
                PlaySelectedCardsAction => PlaySelectedCards(state),
                ScorePresentationFinishedAction => FinishScorePresentation(state),
                DiscardSelectedCardsAction => DiscardCards(state),
                SortHandByRankAction => SortHandByRank(state),
                SortHandBySuitAction => SortHandBySuit(state),
                _ => state
            };
        }

        internal static RoundState PlaySelectedCards(
            RoundState state,
            ScoreResult? overrideScoreResult = null,
            string jokerFeedbackText = null) {
            if (!state.CanPlaySelectedCards) {
                return state;
            }

            var selectedIndexSet = new HashSet<int>(state.SelectedCardsIndexes);
            var playedCards = new List<CardData>();
            var remainingHandCards = new List<CardData>();

            for (int i = 0; i < state.HandCards.Count; i++) {
                if (selectedIndexSet.Contains(i)) {
                    playedCards.Add(state.HandCards[i]);
                } else {
                    remainingHandCards.Add(state.HandCards[i]);
                }
            }

            var handResult = PokerHandEvaluator.Evaluate(playedCards);
            var scoreResult = overrideScoreResult ?? ScoreCalculator.Calculate(playedCards, handResult, state.Blind);
            int newHandsLeft = Math.Max(0, state.HandsLeft - 1);
            int newCurrentScore = state.CurrentScore + scoreResult.FinalScore;
            bool blindCleared = newCurrentScore >= state.TargetScore;
            bool roundLost = !blindCleared && newHandsLeft == 0;

            return new RoundState(
                blind: state.Blind,
                targetScore: state.TargetScore,
                currentScore: newCurrentScore,
                handsLeft: newHandsLeft,
                discardsLeft: state.DiscardsLeft,
                phase: RoundPhase.Scoring,
                maxHandSize: state.MaxHandSize,
                deckCards: state.DeckCards,
                handCards: remainingHandCards,
                discardPileCards: state.DiscardPileCards,
                selectedCardsIndexes: Array.Empty<int>(),
                lastActionText: BuildPlayActionText(handResult.HandType, scoreResult.FinalScore, blindCleared, roundLost, jokerFeedbackText),
                lastPlayedCardsText: FormatPlayedCardsText(playedCards),
                lastPlayedCards: playedCards,
                lastPlayedCardsCount: playedCards.Count,
                lastPlayedHandResult: handResult.HandType,
                lastScoreResult: scoreResult,
                playedCards: playedCards
            );
        }

        private static RoundState ToggleCardSelection(RoundState state, int index) {
            if (state.Phase != RoundPhase.PlayerTurn || index < 0 || index >= state.HandCards.Count) {
                return state;
            }

            var nextSelectedIndexes = new List<int>(state.SelectedCardsIndexes);
            bool wasSelected = nextSelectedIndexes.Contains(index);

            if (wasSelected) {
                nextSelectedIndexes.Remove(index);
            } else {
                if (nextSelectedIndexes.Count >= MaxSelectableCards) {
                    return state;
                }

                nextSelectedIndexes.Add(index);
            }

            nextSelectedIndexes.Sort();

            return CopyWith(
                state,
                selectedCardsIndexes: nextSelectedIndexes,
                lastActionText: wasSelected ? "Card deselected" : "Card selected"
            );
        }

        private static RoundState DiscardCards(RoundState state) {
            if (!state.CanDiscardSelectedCards) {
                return state;
            }

            var selectedIndexSet = new HashSet<int>(state.SelectedCardsIndexes);
            var discardedCards = new List<CardData>();
            var remainingHandCards = new List<CardData>();

            for (int i = 0; i < state.HandCards.Count; i++) {
                if (selectedIndexSet.Contains(i)) {
                    discardedCards.Add(state.HandCards[i]);
                } else {
                    remainingHandCards.Add(state.HandCards[i]);
                }
            }

            int cardsNeeded = Math.Max(0, state.MaxHandSize - remainingHandCards.Count);
            var drawResult = DeckUtility.DrawCards(state.DeckCards, cardsNeeded);

            var newHand = new List<CardData>(remainingHandCards);
            newHand.AddRange(drawResult.DrawnCards);

            var newDiscardPile = new List<CardData>(state.DiscardPileCards);
            newDiscardPile.AddRange(discardedCards);

            int newDiscardsLeft = Math.Max(0, state.DiscardsLeft - 1);
            string lastActionText = newDiscardsLeft == 0
                ? $"Discarded {discardedCards.Count} card(s). No discards left"
                : $"Discarded {discardedCards.Count} card(s)";

            return new RoundState(
                blind: state.Blind,
                targetScore: state.TargetScore,
                currentScore: state.CurrentScore,
                handsLeft: state.HandsLeft,
                discardsLeft: newDiscardsLeft,
                phase: RoundPhase.PlayerTurn,
                maxHandSize: state.MaxHandSize,
                deckCards: drawResult.RemainingDeck,
                handCards: newHand,
                discardPileCards: newDiscardPile,
                selectedCardsIndexes: Array.Empty<int>(),
                lastActionText: lastActionText,
                lastPlayedCardsText: FormatPlayedCardsText(discardedCards),
                lastPlayedCards: Array.Empty<CardData>(),
                lastPlayedCardsCount: discardedCards.Count,
                lastPlayedHandResult: PokerHandType.None,
                lastScoreResult: ScoreResult.Zero
            );
        }

        private static RoundState FinishScorePresentation(RoundState state) {
            if (state.Phase != RoundPhase.Scoring) {
                return state;
            }

            int cardsNeeded = Math.Max(0, state.MaxHandSize - state.HandCards.Count);
            var drawResult = DeckUtility.DrawCards(state.DeckCards, cardsNeeded);

            var newHand = new List<CardData>(state.HandCards);
            newHand.AddRange(drawResult.DrawnCards);

            var newDiscardPile = new List<CardData>(state.DiscardPileCards);
            newDiscardPile.AddRange(state.PlayedCards);

            bool blindCleared = state.CurrentScore >= state.TargetScore;
            bool roundLost = !blindCleared && state.HandsLeft == 0;

            return new RoundState(
                blind: state.Blind,
                targetScore: state.TargetScore,
                currentScore: state.CurrentScore,
                handsLeft: state.HandsLeft,
                discardsLeft: state.DiscardsLeft,
                phase: blindCleared || roundLost ? RoundPhase.RoundEnd : RoundPhase.PlayerTurn,
                maxHandSize: state.MaxHandSize,
                deckCards: drawResult.RemainingDeck,
                handCards: newHand,
                discardPileCards: newDiscardPile,
                selectedCardsIndexes: Array.Empty<int>(),
                lastActionText: state.LastActionText,
                lastPlayedCardsText: state.LastPlayedCardsText,
                lastPlayedCards: state.LastPlayedCards,
                lastPlayedCardsCount: state.LastPlayedCardsCount,
                lastPlayedHandResult: state.LastPlayedHandResult,
                lastScoreResult: state.LastScoreResult,
                playedCards: Array.Empty<CardData>()
            );
        }

        private static RoundState SortHandByRank(RoundState state) {
            if (!state.CanSortHand) {
                return state;
            }

            return CreateSortedState(
                state,
                state.HandCards
                    .OrderByDescending(card => GetRankSortValue(card.Rank))
                    .ThenBy(card => GetSuitSortValue(card.Suit))
                    .ToList(),
                "Sorted by rank"
            );
        }

        private static RoundState SortHandBySuit(RoundState state) {
            if (!state.CanSortHand) {
                return state;
            }

            return CreateSortedState(
                state,
                state.HandCards
                    .OrderBy(card => GetSuitSortValue(card.Suit))
                    .ThenByDescending(card => GetRankSortValue(card.Rank))
                    .ToList(),
                "Sorted by suit"
            );
        }

        private static RoundState CreateSortedState(RoundState state, List<CardData> sortedCards, string actionText) {
            var selectedCards = state.GetSelectedCards();
            var selectedIndexes = new List<int>();

            foreach (var selectedCard in selectedCards) {
                int selectedIndex = sortedCards.IndexOf(selectedCard);
                if (selectedIndex >= 0) {
                    selectedIndexes.Add(selectedIndex);
                }
            }

            selectedIndexes.Sort();

            return CopyWith(
                state,
                handCards: sortedCards,
                selectedCardsIndexes: selectedIndexes,
                lastActionText: actionText
            );
        }

        private static RoundState CopyWith(
            RoundState state,
            IReadOnlyList<CardData> handCards = null,
            IReadOnlyList<CardData> playedCards = null,
            IReadOnlyList<int> selectedCardsIndexes = null,
            string lastActionText = null) {
            return new RoundState(
                blind: state.Blind,
                targetScore: state.TargetScore,
                currentScore: state.CurrentScore,
                handsLeft: state.HandsLeft,
                discardsLeft: state.DiscardsLeft,
                phase: state.Phase,
                maxHandSize: state.MaxHandSize,
                deckCards: state.DeckCards,
                handCards: handCards ?? state.HandCards,
                discardPileCards: state.DiscardPileCards,
                selectedCardsIndexes: selectedCardsIndexes ?? state.SelectedCardsIndexes,
                lastActionText: lastActionText ?? state.LastActionText,
                lastPlayedCardsText: state.LastPlayedCardsText,
                lastPlayedCards: state.LastPlayedCards,
                lastPlayedCardsCount: state.LastPlayedCardsCount,
                lastPlayedHandResult: state.LastPlayedHandResult,
                lastScoreResult: state.LastScoreResult,
                playedCards: playedCards ?? state.PlayedCards
            );
        }

        private static string BuildPlayActionText(PokerHandType handType, int finalScore, bool blindCleared, bool roundLost, string jokerFeedbackText) {
            string handName = FormatHandType(handType);
            string baseText;

            if (blindCleared) {
                baseText = $"Blind cleared with {handName} for {finalScore}";
                return AppendJokerFeedback(baseText, jokerFeedbackText);
            }

            if (roundLost) {
                baseText = $"Round lost with {handName} for {finalScore}";
                return AppendJokerFeedback(baseText, jokerFeedbackText);
            }

            baseText = $"Played {handName} for {finalScore}";
            return AppendJokerFeedback(baseText, jokerFeedbackText);
        }

        private static string AppendJokerFeedback(string baseText, string jokerFeedbackText) {
            return string.IsNullOrWhiteSpace(jokerFeedbackText)
                ? baseText
                : $"{baseText} | Jokers: {jokerFeedbackText}";
        }

        private static int GetRankSortValue(Rank rank) {
            return rank switch {
                Rank.Ace => 14,
                Rank.King => 13,
                Rank.Queen => 12,
                Rank.Jack => 11,
                _ => (int)rank
            };
        }

        private static int GetSuitSortValue(Suit suit) {
            return suit switch {
                Suit.Clubs => 0,
                Suit.Diamonds => 1,
                Suit.Hearts => 2,
                Suit.Spades => 3,
                _ => 99
            };
        }

        private static string FormatPlayedCardsText(IReadOnlyList<CardData> cards) {
            if (cards == null || cards.Count == 0) {
                return "None";
            }

            var parts = new List<string>();

            foreach (var card in cards) {
                parts.Add($"{FormatRank(card.Rank)}{FormatSuit(card.Suit)}");
            }

            return string.Join(", ", parts);
        }

        private static string FormatRank(Rank rank) {
            return rank switch {
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                Rank.Ace => "A",
                Rank.Ten => "10",
                _ => ((int)rank).ToString()
            };
        }

        private static string FormatSuit(Suit suit) {
            return suit switch {
                Suit.Clubs => "\u2663",
                Suit.Diamonds => "\u2666",
                Suit.Hearts => "\u2665",
                Suit.Spades => "\u2660",
                _ => "?"
            };
        }

        private static string FormatHandType(PokerHandType handType) {
            return handType switch {
                PokerHandType.None => "No hand",
                PokerHandType.HighCard => "High Card",
                PokerHandType.Pair => "Pair",
                PokerHandType.TwoPair => "Two Pair",
                PokerHandType.ThreeOfAKind => "Three of a Kind",
                PokerHandType.Straight => "Straight",
                PokerHandType.Flush => "Flush",
                PokerHandType.FullHouse => "Full House",
                PokerHandType.FourOfAKind => "Four of a Kind",
                PokerHandType.StraightFlush => "Straight Flush",
                _ => handType.ToString()
            };
        }
    }
}
