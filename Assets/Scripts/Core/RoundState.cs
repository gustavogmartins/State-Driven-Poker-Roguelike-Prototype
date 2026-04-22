using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core {
    public sealed class RoundState {
        public string BlindName { get; }
        public int TargetScore { get; }
        public int CurrentScore { get; }
        public int HandsLeft { get; }
        public int DiscardsLeft { get; }
        public RoundPhase Phase { get; }
        public IReadOnlyList<CardData> HandCards { get; }
        private IReadOnlyList<int> SelectedCardsIndexes { get; }
        public IReadOnlyList<CardData> DeckCards { get; }
        public string LastPlayedCardsText { get; }
        public int LastPlayedCardsCount { get; }
        public int MaxHandSize { get; }

        public RoundState(string blindName, int targetScore, int currentScore, int handsLeft, int discardsLeft,
            RoundPhase phase, int maxHandSize, IReadOnlyList<CardData> deckCards,IReadOnlyList<CardData> handCards,
            IReadOnlyList<int> selectedCardsIndexes, string lastPlayedCardsText, int lastPlayedCardsCount) {
            BlindName = blindName;
            TargetScore = targetScore;
            CurrentScore = currentScore;
            HandsLeft = handsLeft;
            DiscardsLeft = discardsLeft;
            Phase = phase;
            MaxHandSize = maxHandSize;
            DeckCards = deckCards;
            HandCards = handCards;
            SelectedCardsIndexes = selectedCardsIndexes;
            LastPlayedCardsCount = lastPlayedCardsCount;
            LastPlayedCardsText = lastPlayedCardsText;
        }

        public static RoundState CreateDebug() {
            var fullDeck = DeckBuilder.CreateStandard52();
            var drawResult = DeckUtility.DrawCards(fullDeck, 8);
            
            return new RoundState(
                blindName: "Small blind",
                targetScore: 300,
                currentScore: 0,
                handsLeft: 4,
                discardsLeft: 3,
                phase: RoundPhase.Waiting,
                maxHandSize: 8,
                deckCards: drawResult.RemainingDeck,
                handCards: drawResult.DrawnCards,
                selectedCardsIndexes: new List<int>(),
                lastPlayedCardsText: "None",
                lastPlayedCardsCount: 0
            );
        }

        public RoundState WithScore(int newScore) {
            return new RoundState(
                BlindName,
                TargetScore,
                newScore,
                HandsLeft,
                DiscardsLeft,
                Phase,
                MaxHandSize,
                DeckCards,
                HandCards,
                SelectedCardsIndexes,
                LastPlayedCardsText,
                LastPlayedCardsCount
            );
        }

        public RoundState WithHandsLeft(int handsLeft) {
            return new RoundState(
                BlindName,
                TargetScore,
                CurrentScore,
                handsLeft,
                DiscardsLeft,
                Phase,
                MaxHandSize,
                DeckCards,
                HandCards,
                SelectedCardsIndexes,
                LastPlayedCardsText,
                LastPlayedCardsCount
            );
        }

        public RoundState WithDiscardsLeft(int discardsLeft) {
            return new RoundState(
                BlindName,
                TargetScore,
                CurrentScore,
                HandsLeft,
                discardsLeft,
                Phase,
                MaxHandSize,
                DeckCards,
                HandCards,
                SelectedCardsIndexes,
                LastPlayedCardsText,
                LastPlayedCardsCount
            );
        }

        public RoundState WithPhase(RoundPhase phase) {
            return new RoundState(
                BlindName,
                TargetScore,
                CurrentScore,
                HandsLeft,
                DiscardsLeft,
                phase,
                MaxHandSize,
                DeckCards,
                HandCards,
                SelectedCardsIndexes,
                LastPlayedCardsText,
                LastPlayedCardsCount
            );
        }

        public bool IsSelected(int index) {
            return SelectedCardsIndexes.Contains(index);
        }

        public RoundState ToggleCardSelection(int index) {
            var newSelectedCardsIndexes = new List<int>(SelectedCardsIndexes);

            if (newSelectedCardsIndexes.Contains(index)) {
                newSelectedCardsIndexes.Remove(index);
            } else {
                if (newSelectedCardsIndexes.Count >= 5) {
                    return this;
                }

                newSelectedCardsIndexes.Add(index);
            }

            return new RoundState(
                BlindName,
                TargetScore,
                CurrentScore,
                HandsLeft,
                DiscardsLeft,
                Phase,
                MaxHandSize,
                DeckCards,
                HandCards,
                newSelectedCardsIndexes,
                LastPlayedCardsText,
                LastPlayedCardsCount
            );
        }

        public RoundState PlaySelectedCards() {
            if (SelectedCardsIndexes.Count == 0)
                return this;

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
            
            var cardsNeeded = MaxHandSize - remainingHandCards.Count;
            var drawResult = DeckUtility.DrawCards(DeckCards, cardsNeeded);

            var newHand = new List<CardData>(remainingHandCards);
            newHand.AddRange(drawResult.DrawnCards);

            var playedCardsText = FormatPlayedCardsText(playedCards);
            var newHandsLeft = Mathf.Max(0, HandsLeft - 1);

            return new RoundState(
                blindName: BlindName,
                targetScore: TargetScore,
                currentScore: CurrentScore,
                handsLeft: newHandsLeft,
                discardsLeft: DiscardsLeft,
                phase: RoundPhase.Scoring,
                maxHandSize: MaxHandSize,
                deckCards: drawResult.RemainingDeck,
                handCards: newHand,
                selectedCardsIndexes: new List<int>(),
                lastPlayedCardsText: playedCardsText,
                lastPlayedCardsCount: playedCards.Count
            );
        }

        private static string FormatPlayedCardsText(IReadOnlyList<CardData> cards) {
            if (cards.Count == 0)
                return "None";

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
                Suit.Clubs => "♣",
                Suit.Diamonds => "♦",
                Suit.Hearts => "♥",
                Suit.Spades => "♠",
                _ => "?"
            };
        }
    }
}