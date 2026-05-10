using System;
using System.Collections.Generic;

namespace Core {
    public static class ShopReducer {
        internal static ShopState SelectOffer(ShopState state, int index) {
            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.Offers.Count == 0 || index < 0 || index >= state.Offers.Count || index == state.SelectedOfferIndex) {
                return state;
            }

            return CopyWith(state, selectedOfferIndex: index, selectedOwnedJokerIndex: -1);
        }

        internal static ShopState SelectOwnedJoker(ShopState state, int index, int ownedJokerCount) {
            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }

            if (index < 0 || index >= ownedJokerCount || index == state.SelectedOwnedJokerIndex) {
                return state;
            }

            return CopyWith(state, selectedOwnedJokerIndex: index);
        }

        internal static ShopState PurchaseOffer(ShopState state, string offerId, int updatedMoney) {
            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }

            if (string.IsNullOrWhiteSpace(offerId)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(offerId));
            }

            var nextOffers = new ShopOfferState[state.Offers.Count];
            bool updated = false;

            for (int i = 0; i < state.Offers.Count; i++) {
                ShopOfferState offer = state.Offers[i];
                if (!updated && offer.Id == offerId) {
                    nextOffers[i] = offer.MarkPurchased();
                    updated = !ReferenceEquals(nextOffers[i], offer);
                    continue;
                }

                nextOffers[i] = offer;
            }

            return updated
                ? CopyWith(
                    state,
                    money: updatedMoney,
                    offers: nextOffers,
                    selectedOwnedJokerIndex: -1)
                : state;
        }

        internal static ShopState Reroll(ShopState state, int updatedMoney, IReadOnlyList<JokerState> ownedJokers) {
            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }

            return new ShopState(
                updatedMoney,
                state.NextBlind,
                ownedJokers: ownedJokers,
                selectedOfferIndex: 0,
                selectedOwnedJokerIndex: -1,
                offerPageIndex: state.OfferPageIndex + 1,
                rerollCount: state.RerollCount + 1,
                runSeed: state.RunSeed
            );
        }

        internal static ShopState SyncPurchasedOffers(
            ShopState state,
            IReadOnlyList<JokerState> ownedJokers,
            int updatedMoney) {
            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }

            var nextOffers = new ShopOfferState[state.Offers.Count];
            bool changed = state.Money != updatedMoney;

            for (int i = 0; i < state.Offers.Count; i++) {
                ShopOfferState offer = state.Offers[i];
                bool isOwned = ContainsOwnedJoker(ownedJokers, offer.Id);
                nextOffers[i] = offer.WithPurchasedState(isOwned);
                changed |= !ReferenceEquals(nextOffers[i], offer);
            }

            int selectedOwnedJokerIndex = -1;
            changed |= selectedOwnedJokerIndex != state.SelectedOwnedJokerIndex;

            return changed
                ? CopyWith(
                    state,
                    money: updatedMoney,
                    offers: nextOffers,
                    selectedOwnedJokerIndex: selectedOwnedJokerIndex)
                : state;
        }

        private static ShopState CopyWith(
            ShopState state,
            int? money = null,
            IReadOnlyList<ShopOfferState> offers = null,
            int? selectedOfferIndex = null,
            int? selectedOwnedJokerIndex = null) {
            return new ShopState(
                money ?? state.Money,
                state.NextBlind,
                offers: offers ?? state.Offers,
                selectedOfferIndex: selectedOfferIndex ?? state.SelectedOfferIndex,
                selectedOwnedJokerIndex: selectedOwnedJokerIndex ?? state.SelectedOwnedJokerIndex,
                offerPageIndex: state.OfferPageIndex,
                rerollCount: state.RerollCount,
                runSeed: state.RunSeed
            );
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
