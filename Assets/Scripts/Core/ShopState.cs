using System;
using System.Collections.Generic;

namespace Core {
    public sealed class ShopState {
        private const int BaseRerollCost = 5;

        public int Money { get; }
        public BlindState NextBlind { get; }
        public IReadOnlyList<ShopOfferState> Offers { get; }
        public int SelectedOfferIndex { get; }
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
            OfferPageIndex = Math.Max(0, offerPageIndex);
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
                    offerPageIndex: OfferPageIndex,
                    rerollCount: RerollCount,
                    runSeed: RunSeed)
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
                offerPageIndex: OfferPageIndex,
                rerollCount: RerollCount,
                runSeed: RunSeed);
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
                offerPageIndex: OfferPageIndex,
                rerollCount: RerollCount,
                runSeed: RunSeed);
        }

        public ShopState SelectOffer(int index) {
            if (Offers.Count == 0 || index < 0 || index >= Offers.Count || index == SelectedOfferIndex) {
                return this;
            }

            return new ShopState(
                Money,
                NextBlind,
                offers: Offers,
                selectedOfferIndex: index,
                offerPageIndex: OfferPageIndex,
                rerollCount: RerollCount,
                runSeed: RunSeed);
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
                offerPageIndex: OfferPageIndex + 1,
                rerollCount: RerollCount + 1,
                runSeed: RunSeed
            );
            return rerolledState;
        }

        public ShopState SyncPurchasedOffers(IReadOnlyList<JokerState> ownedJokers, int updatedMoney) {
            var nextOffers = new ShopOfferState[Offers.Count];
            bool changed = Money != updatedMoney;

            for (int i = 0; i < Offers.Count; i++) {
                ShopOfferState offer = Offers[i];
                bool isOwned = ContainsOwnedJoker(ownedJokers, offer.Id);
                nextOffers[i] = offer.WithPurchasedState(isOwned);
                changed |= !ReferenceEquals(nextOffers[i], offer);
            }

            return changed
                ? new ShopState(
                    updatedMoney,
                    NextBlind,
                    offers: nextOffers,
                    selectedOfferIndex: SelectedOfferIndex,
                    offerPageIndex: OfferPageIndex,
                    rerollCount: RerollCount,
                    runSeed: RunSeed)
                : this;
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

        private static bool ContainsOwnedJoker(IReadOnlyList<JokerState> ownedJokers, string jokerId) {
            if (ownedJokers == null) {
                return false;
            }

            for (int i = 0; i < ownedJokers.Count; i++) {
                if (ownedJokers[i].Id == jokerId) {
                    return true;
                }
            }

            return false;
        }
    }
}
