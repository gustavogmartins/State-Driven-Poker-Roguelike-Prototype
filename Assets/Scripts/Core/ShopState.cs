using System;
using System.Collections.Generic;

namespace Core {
    public sealed class ShopState {
        private const int DefaultRerollCost = 1;

        public int Money { get; }
        public BlindState NextBlind { get; }
        public IReadOnlyList<ShopOfferState> Offers { get; }
        public int SelectedOfferIndex { get; }
        public int RerollCount { get; }
        public int RerollCost => DefaultRerollCost;
        public ShopOfferState FirstOffer => Offers.Count > 0 ? Offers[0] : null;
        public ShopOfferState SelectedOffer => Offers.Count > 0 ? Offers[SelectedOfferIndex] : null;

        public ShopState(
            int money,
            BlindState nextBlind,
            IReadOnlyList<ShopOfferState> offers = null,
            IReadOnlyList<JokerState> ownedJokers = null,
            int selectedOfferIndex = 0,
            int rerollCount = 0) {
            if (money < 0) {
                throw new ArgumentOutOfRangeException(nameof(money));
            }

            Money = money;
            NextBlind = nextBlind ?? throw new ArgumentNullException(nameof(nextBlind));
            Offers = offers ?? JokerCatalog.CreateShopOffers(rerollCount, ownedJokers);
            SelectedOfferIndex = ResolveSelectedOfferIndex(Offers, selectedOfferIndex);
            RerollCount = rerollCount;
        }

        public ShopState PurchaseOffer(string offerId, int updatedMoney) {
            if (string.IsNullOrWhiteSpace(offerId)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(offerId));
            }

            var nextOffers = new ShopOfferState[Offers.Count];
            bool updated = false;

            for (int i = 0; i < Offers.Count; i++) {
                ShopOfferState offer = Offers[i];
                if (!updated && offer.Id == offerId) {
                    nextOffers[i] = offer.MarkPurchased();
                    updated = !ReferenceEquals(nextOffers[i], offer);
                    continue;
                }

                nextOffers[i] = offer;
            }

            return updated
                ? new ShopState(
                    updatedMoney,
                    NextBlind,
                    offers: nextOffers,
                    selectedOfferIndex: SelectedOfferIndex,
                    rerollCount: RerollCount)
                : this;
        }

        public ShopState SelectNextOffer() {
            if (Offers.Count <= 1) {
                return this;
            }

            int nextIndex = (SelectedOfferIndex + 1) % Offers.Count;
            return new ShopState(
                Money,
                NextBlind,
                offers: Offers,
                selectedOfferIndex: nextIndex,
                rerollCount: RerollCount);
        }

        public ShopState SelectPreviousOffer() {
            if (Offers.Count <= 1) {
                return this;
            }

            int nextIndex = SelectedOfferIndex == 0
                ? Offers.Count - 1
                : SelectedOfferIndex - 1;

            return new ShopState(
                Money,
                NextBlind,
                offers: Offers,
                selectedOfferIndex: nextIndex,
                rerollCount: RerollCount);
        }

        public bool CanReroll(int money) {
            return money >= RerollCost;
        }

        public ShopState Reroll(int updatedMoney, IReadOnlyList<JokerState> ownedJokers) {
            var rerolledState = new ShopState(
                updatedMoney,
                NextBlind,
                ownedJokers: ownedJokers,
                selectedOfferIndex: 0,
                rerollCount: RerollCount + 1
            );
            return rerolledState;
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
