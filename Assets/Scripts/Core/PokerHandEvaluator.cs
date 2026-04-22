using System.Collections.Generic;

public static class PokerHandEvaluator {
    public static PokerHandResult Evaluate(IReadOnlyList<CardData> cards) {
        if (cards == null || cards.Count == 0)
            return new PokerHandResult(PokerHandType.None);

        var rankCounts = CountRanks(cards);

        int pairCount = CountGroupsOfSize(rankCounts, 2);
        bool hasThreeOfAKind = HasGroupOfSize(rankCounts, 3);
        bool hasFourOfAKind = HasGroupOfSize(rankCounts, 4);

        if (hasFourOfAKind)
            return new PokerHandResult(PokerHandType.FourOfAKind);

        if (hasThreeOfAKind && pairCount >= 1)
            return new PokerHandResult(PokerHandType.FullHouse);

        if (hasThreeOfAKind)
            return new PokerHandResult(PokerHandType.ThreeOfAKind);

        if (pairCount >= 2)
            return new PokerHandResult(PokerHandType.TwoPair);

        if (pairCount == 1)
            return new PokerHandResult(PokerHandType.Pair);

        return new PokerHandResult(PokerHandType.HighCard);
    }

    private static Dictionary<Rank, int> CountRanks(IReadOnlyList<CardData> cards) {
        var counts = new Dictionary<Rank, int>();

        foreach (var card in cards) {
            if (!counts.ContainsKey(card.Rank))
                counts[card.Rank] = 0;

            counts[card.Rank]++;
        }

        return counts;
    }

    private static int CountGroupsOfSize(Dictionary<Rank, int> rankCounts, int groupSize) {
        int count = 0;

        foreach (var entry in rankCounts) {
            if (entry.Value == groupSize)
                count++;
        }

        return count;
    }

    private static bool HasGroupOfSize(Dictionary<Rank, int> rankCounts, int groupSize) {
        foreach (var entry in rankCounts) {
            if (entry.Value == groupSize)
                return true;
        }

        return false;
    }
}