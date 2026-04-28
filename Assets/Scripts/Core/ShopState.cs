using System;
using System.Collections.Generic;

namespace Core {
    public sealed class ShopState {
        public int Money { get; }
        public BlindState NextBlind { get; }
        public IReadOnlyList<ShopOfferState> Offers { get; }
        public int SelectedOfferIndex { get; }
        public ShopOfferState FirstOffer => Offers.Count > 0 ? Offers[0] : null;
        public ShopOfferState SelectedOffer => Offers.Count > 0 ? Offers[SelectedOfferIndex] : null;

        public ShopState(
            int money,
            BlindState nextBlind,
            IReadOnlyList<ShopOfferState> offers = null,
            int selectedOfferIndex = 0) {
            if (money < 0) {
                throw new ArgumentOutOfRangeException(nameof(money));
            }

            Money = money;
            NextBlind = nextBlind ?? throw new ArgumentNullException(nameof(nextBlind));
            Offers = offers ?? CreateDefaultOffers();
            SelectedOfferIndex = ResolveSelectedOfferIndex(Offers, selectedOfferIndex);
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
                ? new ShopState(updatedMoney, NextBlind, nextOffers, SelectedOfferIndex)
                : this;
        }

        public ShopState SelectNextOffer() {
            if (Offers.Count <= 1) {
                return this;
            }

            int nextIndex = (SelectedOfferIndex + 1) % Offers.Count;
            return new ShopState(Money, NextBlind, Offers, nextIndex);
        }

        public ShopState SelectPreviousOffer() {
            if (Offers.Count <= 1) {
                return this;
            }

            int nextIndex = SelectedOfferIndex == 0
                ? Offers.Count - 1
                : SelectedOfferIndex - 1;

            return new ShopState(Money, NextBlind, Offers, nextIndex);
        }

        private static IReadOnlyList<ShopOfferState> CreateDefaultOffers() {
            return new ShopOfferState[] {
                new(
                    id: "glass-joker",
                    title: "Glass Joker",
                    description: "+10 Chips on every scoring hand",
                    cost: 6),
                new(
                    id: "ace-tag",
                    title: "Ace Tag",
                    description: "+4 Mult if hand contains an Ace",
                    cost: 8),
                new(
                    id: "voucher",
                    title: "Clearance Voucher",
                    description: "Reroll stock next milestone",
                    cost: 5)
            };
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
