namespace Core {
    public sealed class RoundState {
        public string BlindName { get; }
        public int TargetScore { get; }
        public int CurrentScore { get; }
        public int HandsLeft { get; }
        public int DiscardsLeft { get; }
        public string Phase { get; }

        public RoundState(string blindName, int targetScore, int currentScore, int handsLeft, int discardsLeft,
            string phase) {
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
                phase: "Waiting"
            );
        }
    }
}