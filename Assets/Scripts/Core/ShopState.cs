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
            int selectedOfferIndex = 0,
            int rerollCount = 0) {
            if (money < 0) {
                throw new ArgumentOutOfRangeException(nameof(money));
            }

            Money = money;
            NextBlind = nextBlind ?? throw new ArgumentNullException(nameof(nextBlind));
            Offers = offers ?? CreateOfferPage(rerollCount);
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
                ? new ShopState(updatedMoney, NextBlind, nextOffers, SelectedOfferIndex, RerollCount)
                : this;
        }

        public ShopState SelectNextOffer() {
            if (Offers.Count <= 1) {
                return this;
            }

            int nextIndex = (SelectedOfferIndex + 1) % Offers.Count;
            return new ShopState(Money, NextBlind, Offers, nextIndex, RerollCount);
        }

        public ShopState SelectPreviousOffer() {
            if (Offers.Count <= 1) {
                return this;
            }

            int nextIndex = SelectedOfferIndex == 0
                ? Offers.Count - 1
                : SelectedOfferIndex - 1;

            return new ShopState(Money, NextBlind, Offers, nextIndex, RerollCount);
        }

        public bool CanReroll(int money) {
            return money >= RerollCost;
        }

        public ShopState Reroll(int updatedMoney, IReadOnlyList<string> ownedOfferIds) {
            var rerolledState = new ShopState(
                updatedMoney,
                NextBlind,
                offers: null,
                selectedOfferIndex: 0,
                rerollCount: RerollCount + 1
            );

            return rerolledState.MarkOwnedOffers(ownedOfferIds);
        }

        public ShopState MarkOwnedOffers(IReadOnlyList<string> ownedOfferIds) {
            if (ownedOfferIds == null || ownedOfferIds.Count == 0) {
                return this;
            }

            var nextOffers = new ShopOfferState[Offers.Count];
            bool changed = false;

            for (int i = 0; i < Offers.Count; i++) {
                ShopOfferState offer = Offers[i];
                bool shouldMarkPurchased = ContainsOwnedOffer(ownedOfferIds, offer.Id);
                nextOffers[i] = shouldMarkPurchased ? offer.MarkPurchased() : offer;
                changed |= !ReferenceEquals(nextOffers[i], offer);
            }

            return changed
                ? new ShopState(Money, NextBlind, nextOffers, SelectedOfferIndex, RerollCount)
                : this;
        }

        private static IReadOnlyList<ShopOfferState> CreateOfferPage(int rerollCount) {
            int pageIndex = rerollCount % 3;

            if (pageIndex == 0) {
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
                        id: "pair-glove",
                        title: "Pair Glove",
                        description: "+20 Chips if the hand is Pair",
                        cost: 5)
                };
            }

            if (pageIndex == 1) {
                return new ShopOfferState[] {
                    new(
                        id: "club-chip",
                        title: "Club Chip",
                        description: "+15 Chips if hand contains a Club",
                        cost: 6),
                    new(
                        id: "straight-polish",
                        title: "Straight Polish",
                        description: "+3 Mult if the hand is Straight",
                        cost: 7),
                    new(
                        id: "heart-tag",
                        title: "Heart Tag",
                        description: "+3 Mult if hand contains a Heart",
                        cost: 6)
                };
            }

            return new ShopOfferState[] {
                new(
                    id: "flush-foil",
                    title: "Flush Foil",
                    description: "+25 Chips if the hand is Flush",
                    cost: 8),
                new(
                    id: "face-card-tag",
                    title: "Face Card Tag",
                    description: "+4 Mult if hand contains J, Q, or K",
                    cost: 7),
                new(
                    id: "two-pair-grip",
                    title: "Two Pair Grip",
                    description: "+18 Chips if the hand is Two Pair",
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

        private static bool ContainsOwnedOffer(IReadOnlyList<string> ownedOfferIds, string offerId) {
            for (int i = 0; i < ownedOfferIds.Count; i++) {
                if (ownedOfferIds[i] == offerId) {
                    return true;
                }
            }

            return false;
        }
    }
}
