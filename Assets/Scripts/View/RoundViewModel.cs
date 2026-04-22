using System.Collections.Generic;

namespace View {
    public class RoundViewModel {
        public string BlindText;
        public string TargetScoreText;
        public string CurrentScoreText;
        public string HandsLeftText;
        public string DiscardsLeftText;
        public string PhaseText;
        public string DeckCountText;
        public List<CardViewModel> HandCards = new();
        public string LastPlayedCountText;
        public string LastPlayedCardsText;
        public string LastActionText;
        public string HandSizeText;
        public string DiscardPileCountText;
        public string TopDiscardText;
    }
}
