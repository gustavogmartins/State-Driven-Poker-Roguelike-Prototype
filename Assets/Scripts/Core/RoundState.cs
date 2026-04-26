using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core {
    public sealed class RoundState {
        private const int MaxSelectableCards = 5;

        public string BlindName { get; }
        public int TargetScore { get; }
        public int CurrentScore { get; }
        public int Money { get; }
        public int Ante { get; }
        public int RoundNumber { get; }
        public int HandsLeft { get; }
        public int DiscardsLeft { get; }
        public RoundPhase Phase { get; }
        public IReadOnlyList<CardData> HandCards { get; }
        public IReadOnlyList<CardData> DeckCards { get; }
        public IReadOnlyList<CardData> DiscardPileCards { get; }
        public IReadOnlyList<int> SelectedCardsIndexes { get; }
        public string LastActionText { get; }
        public string LastPlayedCardsText { get; }
        public IReadOnlyList<CardData> LastPlayedCards { get; }
        public int LastPlayedCardsCount { get; }
        public int MaxHandSize { get; }
        public PokerHandType LastPlayedHandResult { get; }
        public ScoreResult LastScoreResult { get; }
        public int BlindReward => Ante * 10;
        public int RemainingScore => Mathf.Max(0, TargetScore - CurrentScore);
        public int SelectedCardsCount => SelectedCardsIndexes.Count;
        public bool IsRoundOver => Phase == RoundPhase.RoundEnd;
        public bool HasClearedBlind => CurrentScore >= TargetScore;
        public bool HasWonRound => IsRoundOver && HasClearedBlind;
        public bool HasLostRound => IsRoundOver && !HasClearedBlind && HandsLeft == 0;
        public bool CanPlaySelectedCards => !IsRoundOver && SelectedCardsCount > 0 && HandsLeft > 0;
        public bool CanDiscardSelectedCards => !IsRoundOver && SelectedCardsCount > 0 && DiscardsLeft > 0;
        public bool CanSortHand => !IsRoundOver && HandCards.Count > 1;

        public RoundState(
            string blindName,
            int targetScore,
            int currentScore,
            int money,
            int ante,
            int roundNumber,
            int handsLeft,
            int discardsLeft,
            RoundPhase phase,
            int maxHandSize,
            IReadOnlyList<CardData> deckCards,
            IReadOnlyList<CardData> handCards,
            IReadOnlyList<CardData> discardPileCards,
            IReadOnlyList<int> selectedCardsIndexes,
            string lastActionText,
            string lastPlayedCardsText,
            IReadOnlyList<CardData> lastPlayedCards,
            int lastPlayedCardsCount,
            PokerHandType lastPlayedHandResult,
            ScoreResult lastScoreResult) {
            if (string.IsNullOrWhiteSpace(blindName)) {
                throw new ArgumentException("Blind name is required.", nameof(blindName));
            }

            if (targetScore < 0) {
                throw new ArgumentOutOfRangeException(nameof(targetScore));
            }

            if (currentScore < 0) {
                throw new ArgumentOutOfRangeException(nameof(currentScore));
            }

            if (money < 0) {
                throw new ArgumentOutOfRangeException(nameof(money));
            }

            if (ante < 0) {
                throw new ArgumentOutOfRangeException(nameof(ante));
            }

            if (roundNumber < 1) {
                throw new ArgumentOutOfRangeException(nameof(roundNumber));
            }

            if (handsLeft < 0) {
                throw new ArgumentOutOfRangeException(nameof(handsLeft));
            }

            if (discardsLeft < 0) {
                throw new ArgumentOutOfRangeException(nameof(discardsLeft));
            }

            if (maxHandSize <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxHandSize));
            }

            BlindName = blindName;
            TargetScore = targetScore;
            CurrentScore = currentScore;
            Money = money;
            Ante = ante;
            RoundNumber = roundNumber;
            HandsLeft = handsLeft;
            DiscardsLeft = discardsLeft;
            Phase = phase;
            MaxHandSize = maxHandSize;
            DeckCards = CopyCards(deckCards);
            HandCards = CopyCards(handCards);
            DiscardPileCards = CopyCards(discardPileCards);
            SelectedCardsIndexes = SanitizeSelectedIndexes(selectedCardsIndexes, HandCards.Count);
            LastActionText = string.IsNullOrWhiteSpace(lastActionText) ? "Waiting for input" : lastActionText;
            LastPlayedCardsText = string.IsNullOrWhiteSpace(lastPlayedCardsText) ? "None" : lastPlayedCardsText;
            LastPlayedCards = CopyCards(lastPlayedCards);
            LastPlayedCardsCount = Mathf.Max(0, lastPlayedCardsCount);
            LastPlayedHandResult = lastPlayedHandResult;
            LastScoreResult = lastScoreResult;
        }

        public static RoundState CreateInitial(
            string blindName = "Small Blind",
            int targetScore = 300,
            int money = 10,
            int ante = 1,
            int roundNumber = 1,
            int handsLeft = 4,
            int discardsLeft = 3,
            int maxHandSize = 8,
            IReadOnlyList<CardData> initialHandCards = null) {
            var fullDeck = DeckBuilder.CreateStandard52();
            var shuffledDeck = DeckShuffler.Shuffle(fullDeck);

            List<CardData> handCards;
            List<CardData> deckCards;

            if (initialHandCards != null && initialHandCards.Count > 0) {
                handCards = new List<CardData>(initialHandCards.Take(maxHandSize));
                deckCards = RemoveCardsFromDeck(shuffledDeck, handCards);

                int cardsNeeded = Mathf.Max(0, maxHandSize - handCards.Count);
                if (cardsNeeded > 0) {
                    var drawResult = DeckUtility.DrawCards(deckCards, cardsNeeded);
                    handCards.AddRange(drawResult.DrawnCards);
                    deckCards = new List<CardData>(drawResult.RemainingDeck);
                }
            } else {
                var drawResult = DeckUtility.DrawCards(shuffledDeck, maxHandSize);
                handCards = new List<CardData>(drawResult.DrawnCards);
                deckCards = new List<CardData>(drawResult.RemainingDeck);
            }

            return new RoundState(
                blindName: blindName,
                targetScore: targetScore,
                currentScore: 0,
                money: money,
                ante: ante,
                roundNumber: roundNumber,
                handsLeft: handsLeft,
                discardsLeft: discardsLeft,
                phase: RoundPhase.PlayerTurn,
                maxHandSize: maxHandSize,
                deckCards: deckCards,
                handCards: handCards,
                discardPileCards: Array.Empty<CardData>(),
                selectedCardsIndexes: Array.Empty<int>(),
                lastActionText: "Waiting for input",
                lastPlayedCardsText: "None",
                lastPlayedCards: Array.Empty<CardData>(),
                lastPlayedCardsCount: 0,
                lastPlayedHandResult: PokerHandType.None,
                lastScoreResult: ScoreResult.Zero
            );
        }

        public static RoundState CreateDebug() {
            return CreateInitial();
        }

        public bool IsSelected(int index) {
            return SelectedCardsIndexes.Contains(index);
        }

        public IReadOnlyList<CardData> GetSelectedCards() {
            var selectedCards = new List<CardData>();

            foreach (int selectedIndex in SelectedCardsIndexes) {
                if (selectedIndex >= 0 && selectedIndex < HandCards.Count) {
                    selectedCards.Add(HandCards[selectedIndex]);
                }
            }

            return selectedCards;
        }

        public RoundState ToggleCardSelection(int index) {
            if (IsRoundOver || index < 0 || index >= HandCards.Count) {
                return this;
            }

            var newSelectedCardsIndexes = new List<int>(SelectedCardsIndexes);
            bool wasSelected = newSelectedCardsIndexes.Contains(index);

            if (wasSelected) {
                newSelectedCardsIndexes.Remove(index);
            } else {
                if (newSelectedCardsIndexes.Count >= MaxSelectableCards) {
                    return this;
                }

                newSelectedCardsIndexes.Add(index);
            }

            newSelectedCardsIndexes.Sort();

            return CopyWith(
                selectedCardsIndexes: newSelectedCardsIndexes,
                lastActionText: wasSelected ? "Card deselected" : "Card selected"
            );
        }

        public RoundState PlaySelectedCards() {
            if (!CanPlaySelectedCards) {
                return this;
            }

            var selectedIndexSet = new HashSet<int>(SelectedCardsIndexes);
            var playedCards = new List<CardData>();
            var remainingHandCards = new List<CardData>();

            for (int i = 0; i < HandCards.Count; i++) {
                if (selectedIndexSet.Contains(i)) {
                    playedCards.Add(HandCards[i]);
                } else {
                    remainingHandCards.Add(HandCards[i]);
                }
            }

            var handResult = PokerHandEvaluator.Evaluate(playedCards);
            var scoreResult = ScoreCalculator.Calculate(playedCards, handResult);
            int cardsNeeded = Mathf.Max(0, MaxHandSize - remainingHandCards.Count);
            var drawResult = DeckUtility.DrawCards(DeckCards, cardsNeeded);

            var newHand = new List<CardData>(remainingHandCards);
            newHand.AddRange(drawResult.DrawnCards);

            var newDiscardPile = new List<CardData>(DiscardPileCards);
            newDiscardPile.AddRange(playedCards);

            int newHandsLeft = Mathf.Max(0, HandsLeft - 1);
            int newCurrentScore = CurrentScore + scoreResult.FinalScore;
            bool blindCleared = newCurrentScore >= TargetScore;
            bool roundLost = !blindCleared && newHandsLeft == 0;

            return new RoundState(
                blindName: BlindName,
                targetScore: TargetScore,
                currentScore: newCurrentScore,
                money: blindCleared ? Money + BlindReward : Money,
                ante: Ante,
                roundNumber: RoundNumber,
                handsLeft: newHandsLeft,
                discardsLeft: DiscardsLeft,
                phase: blindCleared || roundLost ? RoundPhase.RoundEnd : RoundPhase.PlayerTurn,
                maxHandSize: MaxHandSize,
                deckCards: drawResult.RemainingDeck,
                handCards: newHand,
                discardPileCards: newDiscardPile,
                selectedCardsIndexes: Array.Empty<int>(),
                lastActionText: BuildPlayActionText(handResult.HandType, scoreResult.FinalScore, blindCleared, roundLost),
                lastPlayedCardsText: FormatPlayedCardsText(playedCards),
                lastPlayedCards: playedCards,
                lastPlayedCardsCount: playedCards.Count,
                lastPlayedHandResult: handResult.HandType,
                lastScoreResult: scoreResult
            );
        }

        public RoundState DiscardCards() {
            if (!CanDiscardSelectedCards) {
                return this;
            }

            var selectedIndexSet = new HashSet<int>(SelectedCardsIndexes);
            var discardedCards = new List<CardData>();
            var remainingHandCards = new List<CardData>();

            for (int i = 0; i < HandCards.Count; i++) {
                if (selectedIndexSet.Contains(i)) {
                    discardedCards.Add(HandCards[i]);
                } else {
                    remainingHandCards.Add(HandCards[i]);
                }
            }

            int cardsNeeded = Mathf.Max(0, MaxHandSize - remainingHandCards.Count);
            var drawResult = DeckUtility.DrawCards(DeckCards, cardsNeeded);

            var newHand = new List<CardData>(remainingHandCards);
            newHand.AddRange(drawResult.DrawnCards);

            var newDiscardPile = new List<CardData>(DiscardPileCards);
            newDiscardPile.AddRange(discardedCards);

            int newDiscardsLeft = Mathf.Max(0, DiscardsLeft - 1);
            string lastActionText = newDiscardsLeft == 0
                ? $"Discarded {discardedCards.Count} card(s). No discards left"
                : $"Discarded {discardedCards.Count} card(s)";

            return new RoundState(
                blindName: BlindName,
                targetScore: TargetScore,
                currentScore: CurrentScore,
                money: Money,
                ante: Ante,
                roundNumber: RoundNumber,
                handsLeft: HandsLeft,
                discardsLeft: newDiscardsLeft,
                phase: RoundPhase.PlayerTurn,
                maxHandSize: MaxHandSize,
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

        public RoundState SortHandByRank() {
            if (!CanSortHand) {
                return this;
            }

            return CreateSortedState(
                HandCards
                    .OrderByDescending(card => GetRankSortValue(card.Rank))
                    .ThenBy(card => GetSuitSortValue(card.Suit))
                    .ToList(),
                "Sorted by rank"
            );
        }

        public RoundState SortHandBySuit() {
            if (!CanSortHand) {
                return this;
            }

            return CreateSortedState(
                HandCards
                    .OrderBy(card => GetSuitSortValue(card.Suit))
                    .ThenByDescending(card => GetRankSortValue(card.Rank))
                    .ToList(),
                "Sorted by suit"
            );
        }

        private RoundState CreateSortedState(List<CardData> sortedCards, string actionText) {
            var selectedCards = GetSelectedCards();
            var selectedIndexes = new List<int>();

            foreach (var selectedCard in selectedCards) {
                int selectedIndex = sortedCards.IndexOf(selectedCard);
                if (selectedIndex >= 0) {
                    selectedIndexes.Add(selectedIndex);
                }
            }

            selectedIndexes.Sort();

            return CopyWith(
                handCards: sortedCards,
                selectedCardsIndexes: selectedIndexes,
                lastActionText: actionText
            );
        }

        private RoundState CopyWith(
            IReadOnlyList<CardData> handCards = null,
            IReadOnlyList<int> selectedCardsIndexes = null,
            string lastActionText = null) {
            return new RoundState(
                blindName: BlindName,
                targetScore: TargetScore,
                currentScore: CurrentScore,
                money: Money,
                ante: Ante,
                roundNumber: RoundNumber,
                handsLeft: HandsLeft,
                discardsLeft: DiscardsLeft,
                phase: Phase,
                maxHandSize: MaxHandSize,
                deckCards: DeckCards,
                handCards: handCards ?? HandCards,
                discardPileCards: DiscardPileCards,
                selectedCardsIndexes: selectedCardsIndexes ?? SelectedCardsIndexes,
                lastActionText: lastActionText ?? LastActionText,
                lastPlayedCardsText: LastPlayedCardsText,
                lastPlayedCards: LastPlayedCards,
                lastPlayedCardsCount: LastPlayedCardsCount,
                lastPlayedHandResult: LastPlayedHandResult,
                lastScoreResult: LastScoreResult
            );
        }

        private static IReadOnlyList<CardData> CopyCards(IReadOnlyList<CardData> cards) {
            return new List<CardData>(cards ?? Array.Empty<CardData>()).AsReadOnly();
        }

        private static IReadOnlyList<int> SanitizeSelectedIndexes(IReadOnlyList<int> selectedCardsIndexes, int handCount) {
            if (selectedCardsIndexes == null || selectedCardsIndexes.Count == 0) {
                return Array.Empty<int>();
            }

            var sanitized = selectedCardsIndexes
                .Where(index => index >= 0 && index < handCount)
                .Distinct()
                .OrderBy(index => index)
                .Take(MaxSelectableCards)
                .ToList();

            return sanitized.AsReadOnly();
        }

        private static List<CardData> RemoveCardsFromDeck(List<CardData> deck, IReadOnlyList<CardData> cardsToRemove) {
            var remainingDeck = new List<CardData>(deck);

            foreach (var cardToRemove in cardsToRemove) {
                for (int i = 0; i < remainingDeck.Count; i++) {
                    if (remainingDeck[i].Rank == cardToRemove.Rank &&
                        remainingDeck[i].Suit == cardToRemove.Suit) {
                        remainingDeck.RemoveAt(i);
                        break;
                    }
                }
            }

            return remainingDeck;
        }

        private static string BuildPlayActionText(PokerHandType handType, int finalScore, bool blindCleared, bool roundLost) {
            string handName = FormatHandType(handType);

            if (blindCleared) {
                return $"Blind cleared with {handName} for {finalScore}";
            }

            if (roundLost) {
                return $"Round lost with {handName} for {finalScore}";
            }

            return $"Played {handName} for {finalScore}";
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
