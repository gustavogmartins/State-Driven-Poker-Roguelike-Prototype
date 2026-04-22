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
        public IReadOnlyList<int> SelectedCardsIndexes { get; }
        
        public string LastPlayedCardsText { get; }
        public int LastPlayedCardsCount { get; }


        public RoundState(string blindName, int targetScore, int currentScore, int handsLeft, int discardsLeft,
            RoundPhase phase, IReadOnlyList<CardData> handCards, IReadOnlyList<int> selectedCardsIndexes, string lastPlayedCardsText, int lastPlayedCardsCount) {
            BlindName = blindName;
            TargetScore = targetScore;
            CurrentScore = currentScore;
            HandsLeft = handsLeft;
            DiscardsLeft = discardsLeft;
            Phase = phase;
            HandCards = handCards;
            SelectedCardsIndexes = selectedCardsIndexes;
            LastPlayedCardsCount = lastPlayedCardsCount;
            LastPlayedCardsText = lastPlayedCardsText;
        }

        public static RoundState CreateDebug() {
            return new RoundState(
                blindName: "Small blind",
                targetScore: 300,
                currentScore: 0,
                handsLeft: 4,
                discardsLeft: 3,
                phase: RoundPhase.Waiting,
                handCards: new List<CardData> {
                    new CardData(Rank.Ace, Suit.Spades),
                    new CardData(Rank.Ten, Suit.Hearts),
                    new CardData(Rank.King, Suit.Diamonds),
                    new CardData(Rank.Four, Suit.Clubs),
                    new CardData(Rank.Seven, Suit.Spades),
                    new CardData(Rank.Queen, Suit.Hearts),
                    new CardData(Rank.Two, Suit.Diamonds),
                    new CardData(Rank.Jack, Suit.Clubs)
                },
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
                HandCards,
                newSelectedCardsIndexes,
                LastPlayedCardsText,
                LastPlayedCardsCount
            );
        }
        
        public RoundState PlaySelectedCards()
        {
            if (SelectedCardsIndexes.Count == 0)
                return this;

            var playedCards = new List<CardData>();

            foreach (var selectedIndex in SelectedCardsIndexes)
            {
                if (selectedIndex >= 0 && selectedIndex < HandCards.Count)
                    playedCards.Add(HandCards[selectedIndex]);
            }

            var playedCardsText = FormatPlayedCardsText(playedCards);
            var newHandsLeft = Mathf.Max(0, HandsLeft - 1);

            return new RoundState(
                blindName: BlindName,
                targetScore: TargetScore,
                currentScore: CurrentScore,
                handsLeft: newHandsLeft,
                discardsLeft: DiscardsLeft,
                phase: RoundPhase.Scoring,
                handCards: HandCards,
                selectedCardsIndexes: new List<int>(),
                lastPlayedCardsText: playedCardsText,
                lastPlayedCardsCount: playedCards.Count
            );
        }
        private static string FormatPlayedCardsText(IReadOnlyList<CardData> cards)
        {
            if (cards.Count == 0)
                return "None";

            var parts = new List<string>();

            foreach (var card in cards)
            {
                parts.Add($"{FormatRank(card.Rank)}{FormatSuit(card.Suit)}");
            }

            return string.Join(", ", parts);
        }

        private static string FormatRank(Rank rank)
        {
            return rank switch
            {
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                Rank.Ace => "A",
                Rank.Ten => "10",
                _ => ((int)rank).ToString()
            };
        }

        private static string FormatSuit(Suit suit)
        {
            return suit switch
            {
                Suit.Clubs => "♣",
                Suit.Diamonds => "♦",
                Suit.Hearts => "♥",
                Suit.Spades => "♠",
                _ => "?"
            };
        }
    }
}