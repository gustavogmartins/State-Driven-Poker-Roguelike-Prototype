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
        public string ShopOffersText;
        public string ShopPrimaryActionText;
        public string ShopBuyButtonText;
        public string ShopRerollButtonText;
        public bool CanBuySelectedShopOffer;
        public bool CanRerollShop;
        public bool CanPlayHand;
        public bool CanDiscard;
        public bool CanSort;
        public readonly List<CardViewModel> HandCards = new();
        public readonly List<CardViewModel> PlayedCards = new();
    }
}
