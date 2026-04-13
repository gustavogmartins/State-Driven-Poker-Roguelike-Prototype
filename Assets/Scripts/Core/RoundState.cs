namespace Core {
    public sealed class RoundState {
        public string BlindName { get; }
        public int TargetScore { get; }
        public int CurrentScore { get; }
        public int HandsLeft { get; }
        public int DiscardsLeft { get; }
        public RoundPhaseEnum Phase { get; }

        public RoundState(string blindName, int targetScore, int currentScore, int handsLeft, int discardsLeft,
            RoundPhaseEnum phase) {
            BlindName = blindName;
            TargetScore = targetScore;
            CurrentScore = currentScore;
            HandsLeft = handsLeft;
            DiscardsLeft = discardsLeft;
            Phase = phase;
        }

        public static RoundState CreateDebug() {
            return new RoundState(
                blindName: "Small blind",
                targetScore: 300,
                currentScore: 0,
                handsLeft: 4,
                discardsLeft: 3,
                phase: RoundPhaseEnum.Waiting
            );
        }

        public RoundState WithScore(int newScore) {
            return new RoundState(
                BlindName,
                TargetScore,
                newScore,
                HandsLeft,
                DiscardsLeft,
                Phase
            );
        }

        public RoundState WithHandsLeft(int handsLeft) {
            return new RoundState(
                BlindName,
                TargetScore,
                CurrentScore,
                handsLeft,
                DiscardsLeft,
                Phase
            );
        }

        public RoundState WithDiscardsLeft(int discardsLeft) {
            return new RoundState(
                BlindName,
                TargetScore,
                CurrentScore,
                HandsLeft,
                discardsLeft,
                Phase
            );
        }

        public RoundState WithPhase(RoundPhaseEnum phase) {
            return new RoundState(
                BlindName,
                TargetScore,
                CurrentScore,
                HandsLeft,
                DiscardsLeft,
                phase
                );
        }
    }
}