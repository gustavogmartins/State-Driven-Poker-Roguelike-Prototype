using System.Collections.Generic;
using Core;

public static class ScoreCalculator {
    public static ScoreResult Calculate(
        IReadOnlyList<CardData> playedCards,
        PokerHandResult handResult) {
        return Calculate(playedCards, handResult, blind: null);
    }

    public static ScoreResult Calculate(
        IReadOnlyList<CardData> playedCards,
        PokerHandResult handResult,
        BlindState blind) {
        if (playedCards == null || playedCards.Count == 0)
            return ScoreResult.Zero;

        HandBaseScore baseScore = HandBaseScoreTable.Get(handResult.HandType);
        IReadOnlyList<CardData> scoringCards = ScoringCardSelector.SelectScoringCards(playedCards, handResult);

        int cardChips = 0;
        foreach (CardData card in scoringCards) {
            if (IsDebuffedByBlind(card, blind)) {
                continue;
            }

            cardChips += CardChipValueUtility.GetChipValue(card);
        }

        int totalChips = baseScore.Chips + cardChips;
        int finalScore = totalChips * baseScore.Mult;

        return new ScoreResult(
            baseChips: baseScore.Chips,
            baseMult: baseScore.Mult,
            cardChips: cardChips,
            totalChips: totalChips,
            finalScore: finalScore
        );
    }

    private static bool IsDebuffedByBlind(CardData card, BlindState blind) {
        return blind?.Type == BlindType.Boss && card.Suit == Suit.Clubs;
    }
}
