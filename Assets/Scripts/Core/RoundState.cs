using System.Collections.Generic;

namespace Core {
    public sealed class RoundState {
        public string BlindName { get; }
        public int TargetScore { get; }
        public int CurrentScore { get; }
        public int HandsLeft { get; }
        public int DiscardsLeft { get; }
        public RoundPhase Phase { get; }
        public IReadOnlyList<CardData> HandCards { get; }


        public RoundState(string blindName, int targetScore, int currentScore, int handsLeft, int discardsLeft,
            RoundPhase phase, IReadOnlyList<CardData> handCards) {
            BlindName = blindName;
            TargetScore = targetScore;
            CurrentScore = currentScore;
            HandsLeft = handsLeft;
            DiscardsLeft = discardsLeft;
            Phase = phase;
            HandCards = handCards;
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
                    
                }
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
                HandCards
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
                HandCards
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
                HandCards
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
                HandCards
                );
        }
    }
}