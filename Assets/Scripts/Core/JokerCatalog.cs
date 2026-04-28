using System;
using System.Collections.Generic;

namespace Core {
    public static class JokerCatalog {
        private static readonly IReadOnlyDictionary<string, JokerData> JokerById =
            new Dictionary<string, JokerData>(StringComparer.Ordinal) {
                ["glass-joker"] = new JokerData(
                    id: "glass-joker",
                    name: "Glass Joker",
                    shortCode: "GL",
                    description: "+10 Chips on every scoring hand",
                    cost: 6,
                    bonusType: JokerBonusType.Chips,
                    conditionType: JokerConditionType.Always,
                    bonusValue: 10),
                ["ace-tag"] = new JokerData(
                    id: "ace-tag",
                    name: "Ace Tag",
                    shortCode: "AC",
                    description: "+4 Mult if hand contains an Ace",
                    cost: 8,
                    bonusType: JokerBonusType.Mult,
                    conditionType: JokerConditionType.HandContainsAce,
                    bonusValue: 4),
                ["pair-glove"] = new JokerData(
                    id: "pair-glove",
                    name: "Pair Glove",
                    shortCode: "PG",
                    description: "+20 Chips if the hand is Pair",
                    cost: 5,
                    bonusType: JokerBonusType.Chips,
                    conditionType: JokerConditionType.HandTypePair,
                    bonusValue: 20),
                ["club-chip"] = new JokerData(
                    id: "club-chip",
                    name: "Club Chip",
                    shortCode: "CC",
                    description: "+15 Chips if hand contains a Club",
                    cost: 6,
                    bonusType: JokerBonusType.Chips,
                    conditionType: JokerConditionType.HandContainsClubs,
                    bonusValue: 15),
                ["straight-polish"] = new JokerData(
                    id: "straight-polish",
                    name: "Straight Polish",
                    shortCode: "SP",
                    description: "+3 Mult if the hand is Straight",
                    cost: 7,
                    bonusType: JokerBonusType.Mult,
                    conditionType: JokerConditionType.HandTypeStraight,
                    bonusValue: 3),
                ["heart-tag"] = new JokerData(
                    id: "heart-tag",
                    name: "Heart Tag",
                    shortCode: "HT",
                    description: "+3 Mult if hand contains a Heart",
                    cost: 6,
                    bonusType: JokerBonusType.Mult,
                    conditionType: JokerConditionType.HandContainsHearts,
                    bonusValue: 3),
                ["flush-foil"] = new JokerData(
                    id: "flush-foil",
                    name: "Flush Foil",
                    shortCode: "FF",
                    description: "+25 Chips if the hand is Flush",
                    cost: 8,
                    bonusType: JokerBonusType.Chips,
                    conditionType: JokerConditionType.HandTypeFlush,
                    bonusValue: 25),
                ["face-card-tag"] = new JokerData(
                    id: "face-card-tag",
                    name: "Face Card Tag",
                    shortCode: "FC",
                    description: "+4 Mult if hand contains J, Q, or K",
                    cost: 7,
                    bonusType: JokerBonusType.Mult,
                    conditionType: JokerConditionType.HandContainsFaceCard,
                    bonusValue: 4),
                ["two-pair-grip"] = new JokerData(
                    id: "two-pair-grip",
                    name: "Two Pair Grip",
                    shortCode: "TP",
                    description: "+18 Chips if the hand is Two Pair",
                    cost: 5,
                    bonusType: JokerBonusType.Chips,
                    conditionType: JokerConditionType.HandTypeTwoPair,
                    bonusValue: 18)
            };

        private static readonly string[][] OfferPages = {
            new[] { "glass-joker", "ace-tag", "pair-glove" },
            new[] { "club-chip", "straight-polish", "heart-tag" },
            new[] { "flush-foil", "face-card-tag", "two-pair-grip" }
        };

        public static JokerData GetById(string id) {
            return JokerById.TryGetValue(id, out JokerData joker)
                ? joker
                : throw new KeyNotFoundException($"Unknown joker id '{id}'.");
        }

        public static IReadOnlyList<ShopOfferState> CreateShopOffers(
            int rerollCount,
            IReadOnlyList<JokerState> ownedJokers = null) {
            string[] page = OfferPages[rerollCount % OfferPages.Length];
            var offers = new ShopOfferState[page.Length];

            for (int i = 0; i < page.Length; i++) {
                JokerData joker = GetById(page[i]);
                bool isPurchased = ContainsOwnedJoker(ownedJokers, joker.Id);
                offers[i] = new ShopOfferState(joker, isPurchased);
            }

            return offers;
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
