using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core {
    public sealed class RoundState {
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
        public int SelectedCardsCount => SelectedCardsIndexes.Count;
        public bool CanPlaySelectedCards => Phase != RoundPhase.RoundEnd && SelectedCardsCount > 0 && HandsLeft > 0;
        public bool CanDiscardSelectedCards => Phase != RoundPhase.RoundEnd && SelectedCardsCount > 0 && DiscardsLeft > 0;
        public bool CanSortHand => Phase != RoundPhase.RoundEnd && HandCards.Count > 1;

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
            DeckCards = deckCards;
            HandCards = handCards;
            DiscardPileCards = discardPileCards;
            SelectedCardsIndexes = selectedCardsIndexes;
            LastActionText = lastActionText;
            LastPlayedCardsText = lastPlayedCardsText;
            LastPlayedCards = lastPlayedCards;
            LastPlayedCardsCount = lastPlayedCardsCount;
            LastPlayedHandResult = lastPlayedHandResult;
            LastScoreResult = lastScoreResult;
        }

        public static RoundState CreateDebug() {
            var fullDeck = DeckBuilder.CreateStandard52();
            var shuffledDeck = DeckShuffler.Shuffle(fullDeck);
            var drawResult = DeckUtility.DrawCards(shuffledDeck, 8);

            return new RoundState(
                blindName: "Small Blind",
                targetScore: 300,
                currentScore: 0,
                money: 10,
                ante: 1,
                roundNumber: 1,
                handsLeft: 4,
                discardsLeft: 3,
                phase: RoundPhase.PlayerTurn,
                maxHandSize: 8,
                deckCards: drawResult.RemainingDeck,
                handCards: drawResult.DrawnCards,
                discardPileCards: new List<CardData>(),
                selectedCardsIndexes: new List<int>(),
                lastActionText: "Waiting for input",
                lastPlayedCardsText: "None",
                lastPlayedCards: new List<CardData>(),
                lastPlayedCardsCount: 0,
                lastPlayedHandResult: PokerHandType.None,
                lastScoreResult: ScoreResult.Zero
            );
        }

        public bool IsSelected(int index) {
            return SelectedCardsIndexes.Contains(index);
        }

        public IReadOnlyList<CardData> GetSelectedCards() {
            var selectedCards = new List<CardData>();

            foreach (int selectedIndex in SelectedCardsIndexes.OrderBy(index => index)) {
                if (selectedIndex >= 0 && selectedIndex < HandCards.Count) {
                    selectedCards.Add(HandCards[selectedIndex]);
                }
            }

            return selectedCards;
        }

        public RoundState ToggleCardSelection(int index) {
            if (Phase == RoundPhase.RoundEnd || index < 0 || index >= HandCards.Count) {
                return this;
            }

            var newSelectedCardsIndexes = new List<int>(SelectedCardsIndexes);

            if (newSelectedCardsIndexes.Contains(index)) {
                newSelectedCardsIndexes.Remove(index);
            } else {
                if (newSelectedCardsIndexes.Count >= 5) {
                    return this;
                }

                newSelectedCardsIndexes.Add(index);
            }

            newSelectedCardsIndexes.Sort();

            return CopyWith(
                selectedCardsIndexes: newSelectedCardsIndexes,
                lastActionText: SelectedCardsIndexes.Contains(index)
                    ? "Card deselected"
                    : "Card selected"
            );
        }

        public RoundState PlaySelectedCards() {
            if (!CanPlaySelectedCards) {
                return this;
            }

            var playedCards = new List<CardData>();
            var remainingHandCards = new List<CardData>();

            for (int i = 0; i < HandCards.Count; i++) {
                var card = HandCards[i];

                if (SelectedCardsIndexes.Contains(i)) {
                    playedCards.Add(card);
                } else {
                    remainingHandCards.Add(card);
                }
            }

            var handResult = PokerHandEvaluator.Evaluate(playedCards);
            var scoreResult = ScoreCalculator.Calculate(playedCards, handResult);
            var cardsNeeded = MaxHandSize - remainingHandCards.Count;
            var drawResult = DeckUtility.DrawCards(DeckCards, cardsNeeded);

            var newHand = new List<CardData>(remainingHandCards);
            newHand.AddRange(drawResult.DrawnCards);

            var newDiscardPile = new List<CardData>(DiscardPileCards);
            newDiscardPile.AddRange(playedCards);

            int newHandsLeft = Mathf.Max(0, HandsLeft - 1);
            int newCurrentScore = CurrentScore + scoreResult.FinalScore;
            bool blindCleared = newCurrentScore >= TargetScore;
            int newMoney = blindCleared ? Money + (Ante * 10) : Money;
            RoundPhase newPhase = blindCleared || newHandsLeft == 0 ? RoundPhase.RoundEnd : RoundPhase.PlayerTurn;

            return new RoundState(
                blindName: BlindName,
                targetScore: TargetScore,
                currentScore: newCurrentScore,
                money: newMoney,
                ante: Ante,
                roundNumber: RoundNumber,
                handsLeft: newHandsLeft,
                discardsLeft: DiscardsLeft,
                phase: newPhase,
                maxHandSize: MaxHandSize,
                deckCards: drawResult.RemainingDeck,
                handCards: newHand,
                discardPileCards: newDiscardPile,
                selectedCardsIndexes: new List<int>(),
                lastActionText: blindCleared ? "Blind cleared" : $"Played {FormatHandType(handResult.HandType)}",
                lastPlayedCardsText: FormatPlayedCardsText(playedCards),
                lastPlayedCards: new List<CardData>(playedCards),
                lastPlayedCardsCount: playedCards.Count,
                lastPlayedHandResult: handResult.HandType,
                lastScoreResult: scoreResult
            );
        }

        public RoundState DiscardCards() {
            if (!CanDiscardSelectedCards) {
                return this;
            }

            var discardedCards = new List<CardData>();
            var remainingHandCards = new List<CardData>();

            for (int i = 0; i < HandCards.Count; i++) {
                var card = HandCards[i];

                if (SelectedCardsIndexes.Contains(i)) {
                    discardedCards.Add(card);
                } else {
                    remainingHandCards.Add(card);
                }
            }

            var cardsNeeded = MaxHandSize - remainingHandCards.Count;
            var drawResult = DeckUtility.DrawCards(DeckCards, cardsNeeded);

            var newHand = new List<CardData>(remainingHandCards);
            newHand.AddRange(drawResult.DrawnCards);

            var newDiscardPile = new List<CardData>(DiscardPileCards);
            newDiscardPile.AddRange(discardedCards);

            return new RoundState(
                blindName: BlindName,
                targetScore: TargetScore,
                currentScore: CurrentScore,
                money: Money,
                ante: Ante,
                roundNumber: RoundNumber,
                handsLeft: HandsLeft,
                discardsLeft: Mathf.Max(0, DiscardsLeft - 1),
                phase: RoundPhase.PlayerTurn,
                maxHandSize: MaxHandSize,
                deckCards: drawResult.RemainingDeck,
                handCards: newHand,
                discardPileCards: newDiscardPile,
                selectedCardsIndexes: new List<int>(),
                lastActionText: $"Discarded {discardedCards.Count} card(s)",
                lastPlayedCardsText: FormatPlayedCardsText(discardedCards),
                lastPlayedCards: new List<CardData>(),
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
            if (cards.Count == 0) {
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
