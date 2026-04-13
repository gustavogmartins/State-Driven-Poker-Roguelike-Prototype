using System.Collections.Generic;

namespace View {
    public class RoundViewModel {
        public string BlindText;
        public string TargetScoreText;
        public string CurrentScoreText;
        public string HandsLeftText;
        public string DiscardsLeftText;
        public string PhaseText;

        public List<CardViewModel> HandCards = new();
    }
}
