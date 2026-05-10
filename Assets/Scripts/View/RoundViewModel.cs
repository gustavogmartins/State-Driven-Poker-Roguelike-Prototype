using System.Collections.Generic;

namespace View {
    public class RoundViewModel {
        public string BlindTitleText;
        public string BlindDescriptionText;
        public string BlindRequirementText;
        public string BlindRewardText;
        public string RoundScoreText;
        public string HandNameText;
        public string HandLevelText;
        public string ChipsText;
        public string MultText;
        public string HandsLeftText;
        public string DiscardsLeftText;
        public string MoneyText;
        public string AnteText;
        public string RoundText;
        public string PhaseText;
        public RoundPhase Phase;
        public bool HasScorePresentation;
        public int ScoreStartRoundScore;
        public int ScoreTargetRoundScore;
        public int ScoreBaseChips;
        public int ScoreTargetChips;
        public int ScoreBaseMult;
        public int ScoreTargetBaseMult;
        public int ScoreTargetMultMultiplier;
        public int ScoreFinalScore;
        public int ScoreBonusChips;
        public int ScoreBonusMult;
        public string StatusText;
        public string SelectedCountText;
        public string DeckCountText;
        public string HandSizeText;
        public string TopDiscardText;
        public bool ShowRoundEndOverlay;
        public bool IsWinningRoundEnd;
        public string RoundEndBannerText;
        public string RoundEndSummaryText;
        public string RoundEndDetailsText;
        public string RoundEndPrimaryActionText;
        public bool ShowShopOverlay;
        public string ShopBannerText;
        public string ShopSummaryText;
        public string ShopDetailsText;
        public string ShopPrimaryActionText;
        public string ShopRerollButtonText;
        public bool CanRerollShop;
        public bool CanPlayHand;
        public bool CanDiscard;
        public bool CanSort;
        public readonly List<ShopOfferViewModel> ShopOffers = new();
        public readonly List<CardViewModel> OwnedJokerCards = new();
        public readonly List<CardViewModel> HandCards = new();
        public readonly List<CardViewModel> PlayedCards = new();
        public readonly List<CardViewModel> DiscardedCards = new();

        public IEnumerable<CardViewModel> GameplayCards {
            get {
                foreach (CardViewModel card in HandCards) {
                    yield return card;
                }

                foreach (CardViewModel card in PlayedCards) {
                    yield return card;
                }

                foreach (CardViewModel card in DiscardedCards) {
                    yield return card;
                }
            }
        }
    }
}
