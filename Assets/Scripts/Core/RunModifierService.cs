using System.Collections.Generic;

namespace Core {
    public static class RunModifierService {
        public static ScoreResult ApplyScoreModifiers(
            ScoreResult baseScore,
            IReadOnlyList<JokerState> ownedJokers,
            IReadOnlyList<CardData> playedCards,
            PokerHandResult handResult) {
            if (ownedJokers == null || ownedJokers.Count == 0) {
                return baseScore;
            }

            int totalChips = baseScore.TotalChips;
            int totalMult = baseScore.BaseMult;

            for (int i = 0; i < ownedJokers.Count; i++) {
                JokerState joker = ownedJokers[i];
                if (!MatchesCondition(joker.ConditionType, playedCards, handResult)) {
                    continue;
                }

                if (joker.BonusType == JokerBonusType.Chips) {
                    totalChips += joker.BonusValue;
                    continue;
                }

                if (joker.BonusType == JokerBonusType.Mult) {
                    totalMult += joker.BonusValue;
                }
            }

            return new ScoreResult(
                baseChips: baseScore.BaseChips,
                baseMult: totalMult,
                cardChips: baseScore.CardChips,
                totalChips: totalChips,
                finalScore: totalChips * totalMult
            );
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
