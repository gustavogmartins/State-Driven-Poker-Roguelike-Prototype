using System.Collections.Generic;

public static class PokerHandEvaluator
{
    public static PokerHandResult Evaluate(IReadOnlyList<CardData> cards)
    {
        if (cards == null || cards.Count == 0)
            return new PokerHandResult(PokerHandType.None);

        var rankCounts = CountRanks(cards);
        var pairCount = CountPairs(rankCounts);

        if (pairCount >= 2)
            return new PokerHandResult(PokerHandType.TwoPair);

        if (pairCount == 1)
            return new PokerHandResult(PokerHandType.Pair);

        return new PokerHandResult(PokerHandType.HighCard);
    }

    private static Dictionary<Rank, int> CountRanks(IReadOnlyList<CardData> cards)
    {
        var counts = new Dictionary<Rank, int>();

        foreach (var card in cards)
        {
            if (!counts.ContainsKey(card.Rank))
                counts[card.Rank] = 0;

            counts[card.Rank]++;
        }

        return counts;
    }

    private static int CountPairs(Dictionary<Rank, int> rankCounts)
    {
        int pairCount = 0;

        foreach (var entry in rankCounts)
        {
            if (entry.Value == 2)
                pairCount++;
        }

        return pairCount;
    }
}