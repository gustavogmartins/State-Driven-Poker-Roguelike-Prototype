using System.Collections.Generic;

namespace Core {
    public static class RunModifierService {
        public static ScoreResult ApplyScoreModifiers(
            ScoreResult baseScore,
            IReadOnlyList<string> ownedOfferIds,
            IReadOnlyList<CardData> playedCards) {
            if (ownedOfferIds == null || ownedOfferIds.Count == 0) {
                return baseScore;
            }

            int totalChips = baseScore.TotalChips;
            int totalMult = baseScore.BaseMult;

            for (int i = 0; i < ownedOfferIds.Count; i++) {
                string offerId = ownedOfferIds[i];

                if (offerId == "glass-joker") {
                    totalChips += 10;
                    continue;
                }

                if (offerId == "ace-tag" && HandContainsAce(playedCards)) {
                    totalMult += 4;
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
    }
}
