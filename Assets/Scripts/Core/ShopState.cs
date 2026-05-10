using System;
using System.Collections.Generic;

namespace Core {
    public sealed class ShopState {
        private const int BaseRerollCost = 5;

        public int Money { get; }
        public BlindState NextBlind { get; }
        public IReadOnlyList<ShopOfferState> Offers { get; }
        public int SelectedOfferIndex { get; }
        public int SelectedOwnedJokerIndex { get; }
        public int OfferPageIndex { get; }
        public int RerollCount { get; }
        public int RunSeed { get; }
        public int RerollCost => BaseRerollCost + RerollCount;
        public ShopOfferState FirstOffer => Offers.Count > 0 ? Offers[0] : null;
        public ShopOfferState SelectedOffer => Offers.Count > 0 ? Offers[SelectedOfferIndex] : null;

        public ShopState(
            int money,
            BlindState nextBlind,
            IReadOnlyList<ShopOfferState> offers = null,
            IReadOnlyList<JokerState> ownedJokers = null,
            int selectedOfferIndex = 0,
            int selectedOwnedJokerIndex = -1,
            int offerPageIndex = 0,
            int rerollCount = 0,
            int runSeed = 0) {
            if (money < 0) {
                throw new ArgumentOutOfRangeException(nameof(money));
            }

            Money = money;
            NextBlind = nextBlind ?? throw new ArgumentNullException(nameof(nextBlind));
            RunSeed = runSeed;
            Offers = offers ?? JokerCatalog.CreateShopOffers(offerPageIndex, ownedJokers, RunSeed);
            SelectedOfferIndex = ResolveSelectedOfferIndex(Offers, selectedOfferIndex);
            SelectedOwnedJokerIndex = selectedOwnedJokerIndex;
            OfferPageIndex = Math.Max(0, offerPageIndex);
            RerollCount = rerollCount;
        }

        public bool CanReroll(int money) {
            return money >= RerollCost;
        }

        private static int ResolveSelectedOfferIndex(IReadOnlyList<ShopOfferState> offers, int selectedOfferIndex) {
            if (offers == null || offers.Count == 0) {
                return 0;
            }

            if (selectedOfferIndex < 0) {
                return 0;
            }

            if (selectedOfferIndex >= offers.Count) {
                return offers.Count - 1;
            }

            return selectedOfferIndex;
        }
    }
}
