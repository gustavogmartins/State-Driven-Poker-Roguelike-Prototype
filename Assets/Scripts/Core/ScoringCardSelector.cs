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

        return handResult.HandType switch {
            PokerHandType.Pair => SelectCardsInRankGroups(playedCards, groupSize: 2),
            PokerHandType.TwoPair => SelectCardsInRankGroups(playedCards, groupSize: 2),
            PokerHandType.ThreeOfAKind => SelectCardsInRankGroups(playedCards, groupSize: 3),
            PokerHandType.FourOfAKind => SelectCardsInRankGroups(playedCards, groupSize: 4),
            PokerHandType.FullHouse => new List<CardData>(playedCards),
            PokerHandType.Straight => new List<CardData>(playedCards),
            PokerHandType.Flush => new List<CardData>(playedCards),
            PokerHandType.StraightFlush => new List<CardData>(playedCards),
            _ => new List<CardData>()
        };
    }

    private static List<CardData> SelectCardsInRankGroups(IReadOnlyList<CardData> playedCards, int groupSize) {
        return playedCards
            .GroupBy(card => card.Rank)
            .Where(group => group.Count() == groupSize)
            .OrderByDescending(group => GetHighCardSortValue(group.First()))
            .SelectMany(group => group)
            .ToList();
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
