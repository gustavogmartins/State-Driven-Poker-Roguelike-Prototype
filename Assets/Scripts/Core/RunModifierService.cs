using System.Collections.Generic;

namespace Core {
    public readonly struct JokerModifierResult {
        public ScoreResult ScoreResult { get; }
        public int MoneyBonus { get; }
        public string TriggeredText { get; }

        public JokerModifierResult(ScoreResult scoreResult, int moneyBonus, string triggeredText) {
            ScoreResult = scoreResult;
            MoneyBonus = moneyBonus;
            TriggeredText = triggeredText;
        }
    }

    public static class RunModifierService {
        public static ScoreResult ApplyScoreModifiers(
            ScoreResult baseScore,
            IReadOnlyList<JokerState> ownedJokers,
            IReadOnlyList<CardData> playedCards,
            PokerHandResult handResult) {
            return ApplyModifiers(baseScore, ownedJokers, playedCards, handResult).ScoreResult;
        }

        public static JokerModifierResult ApplyModifiers(
            ScoreResult baseScore,
            IReadOnlyList<JokerState> ownedJokers,
            IReadOnlyList<CardData> playedCards,
            PokerHandResult handResult) {
            if (ownedJokers == null || ownedJokers.Count == 0) {
                return new JokerModifierResult(baseScore, 0, string.Empty);
            }

            int totalChips = baseScore.TotalChips;
            int totalMult = baseScore.BaseMult;
            int multMultiplier = baseScore.MultMultiplier;
            int moneyBonus = 0;
            var triggeredEffects = new List<string>();

            for (int i = 0; i < ownedJokers.Count; i++) {
                JokerState joker = ownedJokers[i];
                if (!MatchesCondition(joker.ConditionType, playedCards, handResult)) {
                    continue;
                }

                switch (joker.BonusType) {
                    case JokerBonusType.Chips:
                        totalChips += joker.BonusValue;
                        triggeredEffects.Add($"{joker.Name} +{joker.BonusValue} Chips");
                        break;
                    case JokerBonusType.Mult:
                        totalMult += joker.BonusValue;
                        triggeredEffects.Add($"{joker.Name} +{joker.BonusValue} Mult");
                        break;
                    case JokerBonusType.XMult:
                        multMultiplier *= joker.BonusValue;
                        triggeredEffects.Add($"{joker.Name} x{joker.BonusValue}");
                        break;
                    case JokerBonusType.Money:
                        moneyBonus += joker.BonusValue;
                        triggeredEffects.Add($"{joker.Name} +${joker.BonusValue}");
                        break;
                }
            }

            ScoreResult modifiedScore = new ScoreResult(
                baseChips: baseScore.BaseChips,
                baseMult: totalMult,
                cardChips: baseScore.CardChips,
                totalChips: totalChips,
                finalScore: totalChips * totalMult * multMultiplier,
                multMultiplier: multMultiplier
            );

            string triggeredText = triggeredEffects.Count > 0
                ? string.Join(", ", triggeredEffects)
                : string.Empty;

            return new JokerModifierResult(modifiedScore, moneyBonus, triggeredText);
        }

        private static bool MatchesCondition(
            JokerConditionType conditionType,
            IReadOnlyList<CardData> playedCards,
            PokerHandResult handResult) {
            return conditionType switch {
                JokerConditionType.Always => true,
                JokerConditionType.HandContainsAce => HandContainsAce(playedCards),
                JokerConditionType.HandTypePair => handResult.HandType == PokerHandType.Pair,
                JokerConditionType.HandContainsClubs => HandContainsSuit(playedCards, Suit.Clubs),
                JokerConditionType.HandTypeStraight => handResult.HandType == PokerHandType.Straight,
                JokerConditionType.HandContainsHearts => HandContainsSuit(playedCards, Suit.Hearts),
                JokerConditionType.HandTypeFlush => handResult.HandType == PokerHandType.Flush,
                JokerConditionType.HandContainsFaceCard => HandContainsFaceCard(playedCards),
                JokerConditionType.HandTypeTwoPair => handResult.HandType == PokerHandType.TwoPair,
                JokerConditionType.HandContainsSpades => HandContainsSuit(playedCards, Suit.Spades),
                JokerConditionType.HandTypeThreeOfAKind => handResult.HandType == PokerHandType.ThreeOfAKind,
                JokerConditionType.HandTypeFullHouse => handResult.HandType == PokerHandType.FullHouse,
                _ => false
            };
        }

        private static bool HandContainsAce(IReadOnlyList<CardData> playedCards) {
            if (playedCards == null) {
                return false;
            }

            for (int i = 0; i < playedCards.Count; i++) {
                if (playedCards[i].Rank == Rank.Ace) {
                    return true;
                }
            }

            return false;
        }

        private static bool HandContainsSuit(IReadOnlyList<CardData> playedCards, Suit suit) {
            if (playedCards == null) {
                return false;
            }

            for (int i = 0; i < playedCards.Count; i++) {
                if (playedCards[i].Suit == suit) {
                    return true;
                }
            }

            return false;
        }

        private static bool HandContainsFaceCard(IReadOnlyList<CardData> playedCards) {
            if (playedCards == null) {
                return false;
            }

            for (int i = 0; i < playedCards.Count; i++) {
                Rank rank = playedCards[i].Rank;
                if (rank == Rank.Jack || rank == Rank.Queen || rank == Rank.King) {
                    return true;
                }
            }

            return false;
        }
    }
}
