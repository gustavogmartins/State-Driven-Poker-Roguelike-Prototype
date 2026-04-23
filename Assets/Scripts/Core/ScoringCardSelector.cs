using System.Collections.Generic;
using System.Linq;

public static class ScoringCardSelector {
    public static IReadOnlyList<CardData> SelectScoringCards(
        IReadOnlyList<CardData> playedCards,
        PokerHandResult handResult) {
        if (playedCards == null || playedCards.Count == 0)
            return new List<CardData>();

        if (handResult.HandType == PokerHandType.HighCard) {
            CardData highestCard = playedCards
                .OrderByDescending(GetHighCardSortValue)
                .First();

            return new List<CardData> { highestCard };
        }

        return new List<CardData>(playedCards);
    }

    private static int GetHighCardSortValue(CardData card) {
        return card.Rank switch {
            Rank.Ace => 14,
            Rank.King => 13,
            Rank.Queen => 12,
            Rank.Jack => 11,
            _ => (int)card.Rank
        };
    }
}