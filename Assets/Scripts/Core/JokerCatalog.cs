using System;
using System.Collections.Generic;

namespace Core {
    public static class JokerCatalog {
        private const int ShopOfferCount = 3;

        private static readonly JokerData[] Jokers = {
            new(
                id: "glass-joker",
                name: "Glass Joker",
                shortCode: "GL",
                description: "+10 Chips on every scoring hand",
                cost: 6,
                rarity: JokerRarity.Common,
                bonusType: JokerBonusType.Chips,
                conditionType: JokerConditionType.Always,
                bonusValue: 10),
            new(
                id: "ace-tag",
                name: "Ace Tag",
                shortCode: "AC",
                description: "+4 Mult if hand contains an Ace",
                cost: 8,
                rarity: JokerRarity.Uncommon,
                bonusType: JokerBonusType.Mult,
                conditionType: JokerConditionType.HandContainsAce,
                bonusValue: 4),
            new(
                id: "pair-glove",
                name: "Pair Glove",
                shortCode: "PG",
                description: "+20 Chips if the hand is Pair",
                cost: 5,
                rarity: JokerRarity.Common,
                bonusType: JokerBonusType.Chips,
                conditionType: JokerConditionType.HandTypePair,
                bonusValue: 20),
            new(
                id: "club-chip",
                name: "Club Chip",
                shortCode: "CC",
                description: "+15 Chips if hand contains a Club",
                cost: 6,
                rarity: JokerRarity.Common,
                bonusType: JokerBonusType.Chips,
                conditionType: JokerConditionType.HandContainsClubs,
                bonusValue: 15),
            new(
                id: "straight-polish",
                name: "Straight Polish",
                shortCode: "SP",
                description: "+3 Mult if the hand is Straight",
                cost: 7,
                rarity: JokerRarity.Uncommon,
                bonusType: JokerBonusType.Mult,
                conditionType: JokerConditionType.HandTypeStraight,
                bonusValue: 3),
            new(
                id: "heart-tag",
                name: "Heart Tag",
                shortCode: "HT",
                description: "+3 Mult if hand contains a Heart",
                cost: 6,
                rarity: JokerRarity.Common,
                bonusType: JokerBonusType.Mult,
                conditionType: JokerConditionType.HandContainsHearts,
                bonusValue: 3),
            new(
                id: "spade-token",
                name: "Spade Token",
                shortCode: "ST",
                description: "+15 Chips if hand contains a Spade",
                cost: 5,
                rarity: JokerRarity.Common,
                bonusType: JokerBonusType.Chips,
                conditionType: JokerConditionType.HandContainsSpades,
                bonusValue: 15),
            new(
                id: "cash-tag",
                name: "Cash Tag",
                shortCode: "CT",
                description: "+$2 if hand contains an Ace",
                cost: 6,
                rarity: JokerRarity.Common,
                bonusType: JokerBonusType.Money,
                conditionType: JokerConditionType.HandContainsAce,
                bonusValue: 2),
            new(
                id: "discard-pass",
                name: "Discard Pass",
                shortCode: "DP",
                description: "+1 discard per blind",
                cost: 6,
                rarity: JokerRarity.Common,
                bonusType: JokerBonusType.ExtraDiscard,
                conditionType: JokerConditionType.Always,
                bonusValue: 1),
            new(
                id: "flush-foil",
                name: "Flush Foil",
                shortCode: "FF",
                description: "+25 Chips if the hand is Flush",
                cost: 10,
                rarity: JokerRarity.Rare,
                bonusType: JokerBonusType.Chips,
                conditionType: JokerConditionType.HandTypeFlush,
                bonusValue: 25),
            new(
                id: "face-card-tag",
                name: "Face Card Tag",
                shortCode: "FC",
                description: "+4 Mult if hand contains J, Q, or K",
                cost: 7,
                rarity: JokerRarity.Uncommon,
                bonusType: JokerBonusType.Mult,
                conditionType: JokerConditionType.HandContainsFaceCard,
                bonusValue: 4),
            new(
                id: "two-pair-grip",
                name: "Two Pair Grip",
                shortCode: "TP",
                description: "+18 Chips if the hand is Two Pair",
                cost: 5,
                rarity: JokerRarity.Common,
                bonusType: JokerBonusType.Chips,
                conditionType: JokerConditionType.HandTypeTwoPair,
                bonusValue: 18),
            new(
                id: "triple-grip",
                name: "Triple Grip",
                shortCode: "TG",
                description: "+5 Mult if the hand is Three of a Kind",
                cost: 8,
                rarity: JokerRarity.Uncommon,
                bonusType: JokerBonusType.Mult,
                conditionType: JokerConditionType.HandTypeThreeOfAKind,
                bonusValue: 5),
            new(
                id: "straight-engine",
                name: "Straight Engine",
                shortCode: "SE",
                description: "x2 Mult if the hand is Straight",
                cost: 9,
                rarity: JokerRarity.Uncommon,
                bonusType: JokerBonusType.XMult,
                conditionType: JokerConditionType.HandTypeStraight,
                bonusValue: 2),
            new(
                id: "pair-payout",
                name: "Pair Payout",
                shortCode: "PP",
                description: "+$3 if the hand is Pair",
                cost: 7,
                rarity: JokerRarity.Uncommon,
                bonusType: JokerBonusType.Money,
                conditionType: JokerConditionType.HandTypePair,
                bonusValue: 3),
            new(
                id: "flush-mirror",
                name: "Flush Mirror",
                shortCode: "FM",
                description: "x2 Mult if the hand is Flush",
                cost: 11,
                rarity: JokerRarity.Rare,
                bonusType: JokerBonusType.XMult,
                conditionType: JokerConditionType.HandTypeFlush,
                bonusValue: 2),
            new(
                id: "full-house-vault",
                name: "Full House Vault",
                shortCode: "FH",
                description: "+$5 if the hand is Full House",
                cost: 12,
                rarity: JokerRarity.Rare,
                bonusType: JokerBonusType.Money,
                conditionType: JokerConditionType.HandTypeFullHouse,
                bonusValue: 5),
            new(
                id: "spare-hand",
                name: "Spare Hand",
                shortCode: "SH",
                description: "+1 hand per blind",
                cost: 12,
                rarity: JokerRarity.Rare,
                bonusType: JokerBonusType.ExtraHand,
                conditionType: JokerConditionType.Always,
                bonusValue: 1)
        };

        private static readonly IReadOnlyDictionary<string, JokerData> JokerById = BuildJokerById();

        public static IReadOnlyList<JokerData> All => Jokers;

        public static JokerData GetById(string id) {
            return JokerById.TryGetValue(id, out JokerData joker)
                ? joker
                : throw new KeyNotFoundException($"Unknown joker id '{id}'.");
        }

        public static IReadOnlyList<ShopOfferState> CreateShopOffers(
            int offerPageIndex,
            IReadOnlyList<JokerState> ownedJokers = null,
            int runSeed = 0) {
            var selectedJokers = new List<JokerData>(ShopOfferCount);
            var unownedCandidates = BuildCandidates(excludeOwned: true, ownedJokers);
            var ownedCandidates = BuildCandidates(excludeOwned: false, ownedJokers);
            var random = new Random(CombineSeed(runSeed, offerPageIndex));

            for (int i = 0; i < ShopOfferCount; i++) {
                List<JokerData> candidatePool = unownedCandidates.Count > 0
                    ? unownedCandidates
                    : ownedCandidates;

                JokerData selectedJoker = PickWeighted(candidatePool, random);
                candidatePool.Remove(selectedJoker);
                selectedJokers.Add(selectedJoker);
            }

            var offers = new ShopOfferState[selectedJokers.Count];
            for (int i = 0; i < selectedJokers.Count; i++) {
                JokerData joker = selectedJokers[i];
                offers[i] = new ShopOfferState(joker, ContainsOwnedJoker(ownedJokers, joker.Id));
            }

            return offers;
        }

        private static IReadOnlyDictionary<string, JokerData> BuildJokerById() {
            var jokersById = new Dictionary<string, JokerData>(StringComparer.Ordinal);
            for (int i = 0; i < Jokers.Length; i++) {
                jokersById.Add(Jokers[i].Id, Jokers[i]);
            }

            return jokersById;
        }

        private static List<JokerData> BuildCandidates(bool excludeOwned, IReadOnlyList<JokerState> ownedJokers) {
            var candidates = new List<JokerData>(Jokers.Length);
            for (int i = 0; i < Jokers.Length; i++) {
                if (excludeOwned && ContainsOwnedJoker(ownedJokers, Jokers[i].Id)) {
                    continue;
                }

                if (!excludeOwned && !ContainsOwnedJoker(ownedJokers, Jokers[i].Id)) {
                    continue;
                }

                candidates.Add(Jokers[i]);
            }

            return candidates;
        }

        private static JokerData PickWeighted(IReadOnlyList<JokerData> candidates, Random random) {
            int totalWeight = 0;
            for (int i = 0; i < candidates.Count; i++) {
                totalWeight += GetRarityWeight(candidates[i].Rarity);
            }

            int roll = random.Next(totalWeight);
            for (int i = 0; i < candidates.Count; i++) {
                roll -= GetRarityWeight(candidates[i].Rarity);
                if (roll < 0) {
                    return candidates[i];
                }
            }

            return candidates[candidates.Count - 1];
        }

        private static int GetRarityWeight(JokerRarity rarity) {
            return rarity switch {
                JokerRarity.Common => 70,
                JokerRarity.Uncommon => 25,
                JokerRarity.Rare => 5,
                _ => 1
            };
        }

        private static int CombineSeed(int runSeed, int offerPageIndex) {
            unchecked {
                return (runSeed * 397) ^ offerPageIndex;
            }
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
