using System.Collections.Generic;

public static class ScoreCalculator {
    public static ScoreResult Calculate(
        IReadOnlyList<CardData> playedCards,
        PokerHandResult handResult) {
        if (playedCards == null || playedCards.Count == 0)
            return ScoreResult.Zero;

        HandBaseScore baseScore = HandBaseScoreTable.Get(handResult.HandType);
        IReadOnlyList<CardData> scoringCards = ScoringCardSelector.SelectScoringCards(playedCards, handResult);

        int cardChips = 0;
        foreach (CardData card in scoringCards) {
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
}