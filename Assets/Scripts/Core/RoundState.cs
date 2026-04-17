using System.Collections.Generic;
using System.Linq;

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


        public RoundState(string blindName, int targetScore, int currentScore, int handsLeft, int discardsLeft,
            RoundPhase phase, IReadOnlyList<CardData> handCards, IReadOnlyList<int> selectedCardsIndexes) {
            BlindName = blindName;
            TargetScore = targetScore;
            CurrentScore = currentScore;
            HandsLeft = handsLeft;
            DiscardsLeft = discardsLeft;
            Phase = phase;
            HandCards = handCards;
            SelectedCardsIndexes = selectedCardsIndexes;
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
                selectedCardsIndexes: new List<int>()
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
                SelectedCardsIndexes
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
                SelectedCardsIndexes
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
                SelectedCardsIndexes
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
                SelectedCardsIndexes
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
                newSelectedCardsIndexes
            );
        }
    }
}