using System.Collections.Generic;
using Core;

public readonly struct ScoringCardContribution {
    public ScoringCardContribution(CardData card, int chipValue) {
        Card = card;
        ChipValue = chipValue;
    }

    public CardData Card { get; }
    public int ChipValue { get; }
}

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
        int cardChips = 0;
        foreach (ScoringCardContribution contribution in GetScoringCardContributions(playedCards, handResult, blind)) {
            cardChips += contribution.ChipValue;
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

    public static IReadOnlyList<ScoringCardContribution> GetScoringCardContributions(
        IReadOnlyList<CardData> playedCards,
        PokerHandResult handResult) {
        return GetScoringCardContributions(playedCards, handResult, blind: null);
    }

    public static IReadOnlyList<ScoringCardContribution> GetScoringCardContributions(
        IReadOnlyList<CardData> playedCards,
        PokerHandResult handResult,
        BlindState blind) {
        var contributions = new List<ScoringCardContribution>();
        IReadOnlyList<CardData> scoringCards = ScoringCardSelector.SelectScoringCards(playedCards, handResult);

        foreach (CardData card in scoringCards) {
            if (IsDebuffedByBlind(card, blind)) {
                continue;
            }

            contributions.Add(new ScoringCardContribution(card, CardChipValueUtility.GetChipValue(card)));
        }

        return contributions;
    }

    private static bool IsDebuffedByBlind(CardData card, BlindState blind) {
        return blind?.Type == BlindType.Boss && card.Suit == Suit.Clubs;
    }
}
