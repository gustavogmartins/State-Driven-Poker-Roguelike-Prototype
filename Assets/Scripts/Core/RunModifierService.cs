using System.Collections.Generic;

namespace Core {
    public static class RunModifierService {
        public static ScoreResult ApplyScoreModifiers(
            ScoreResult baseScore,
            IReadOnlyList<string> ownedOfferIds,
            IReadOnlyList<CardData> playedCards,
            PokerHandResult handResult) {
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
                    continue;
                }

                if (offerId == "pair-glove" && handResult.HandType == PokerHandType.Pair) {
                    totalChips += 20;
                    continue;
                }

                if (offerId == "club-chip" && HandContainsSuit(playedCards, Suit.Clubs)) {
                    totalChips += 15;
                    continue;
                }

                if (offerId == "straight-polish" && handResult.HandType == PokerHandType.Straight) {
                    totalMult += 3;
                    continue;
                }

                if (offerId == "heart-tag" && HandContainsSuit(playedCards, Suit.Hearts)) {
                    totalMult += 3;
                    continue;
                }

                if (offerId == "flush-foil" && handResult.HandType == PokerHandType.Flush) {
                    totalChips += 25;
                    continue;
                }

                if (offerId == "face-card-tag" && HandContainsFaceCard(playedCards)) {
                    totalMult += 4;
                    continue;
                }

                if (offerId == "two-pair-grip" && handResult.HandType == PokerHandType.TwoPair) {
                    totalChips += 18;
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
