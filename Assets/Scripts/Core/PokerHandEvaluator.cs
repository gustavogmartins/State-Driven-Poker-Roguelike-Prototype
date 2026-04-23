using System.Collections.Generic;

public static class PokerHandEvaluator {
    public static PokerHandResult Evaluate(IReadOnlyList<CardData> cards) {
        if (cards == null || cards.Count == 0)
            return new PokerHandResult(PokerHandType.None);

        var rankCounts = CountRanks(cards);

        int pairCount = CountGroupsOfSize(rankCounts, 2);
        bool hasThreeOfAKind = HasGroupOfSize(rankCounts, 3);
        bool hasFourOfAKind = HasGroupOfSize(rankCounts, 4);
        bool isFlush = IsFlush(cards);
        bool isStraight = TryGetStraightInfo(cards, out bool isAceLowStraight);

        if (isStraight && isFlush)
            return new PokerHandResult(PokerHandType.StraightFlush, isAceLowStraight);

        if (hasFourOfAKind)
            return new PokerHandResult(PokerHandType.FourOfAKind);

        if (hasThreeOfAKind && pairCount >= 1)
            return new PokerHandResult(PokerHandType.FullHouse);

        if (isFlush)
            return new PokerHandResult(PokerHandType.Flush);

        if (isStraight)
            return new PokerHandResult(PokerHandType.Straight, isAceLowStraight);

        if (hasThreeOfAKind)
            return new PokerHandResult(PokerHandType.ThreeOfAKind);

        if (pairCount >= 2)
            return new PokerHandResult(PokerHandType.TwoPair);

        if (pairCount == 1)
            return new PokerHandResult(PokerHandType.Pair);

        return new PokerHandResult(PokerHandType.HighCard);
    }

    private static bool TryGetStraightInfo(IReadOnlyList<CardData> cards, out bool isAceLowStraight) {
        isAceLowStraight = false;

        if (cards == null || cards.Count != 5)
            return false;

        var values = GetSortedUniqueRankValues(cards);

        if (values.Count != 5)
            return false;

        if (IsConsecutive(values))
            return true;

        if (values.Contains((int)Rank.Ace)) {
            var aceLowValues = new List<int>(values);
            int aceIndex = aceLowValues.IndexOf((int)Rank.Ace);
            aceLowValues[aceIndex] = 1;
            aceLowValues.Sort();

            if (IsConsecutive(aceLowValues)) {
                isAceLowStraight = true;
                return true;
            }
        }

        return false;
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

    private static bool IsFlush(IReadOnlyList<CardData> cards) {
        if (cards == null || cards.Count != 5)
            return false;

        var firstSuit = cards[0].Suit;

        foreach (var card in cards) {
            if (card.Suit != firstSuit)
                return false;
        }

        return true;
    }

    private static bool IsStraight(IReadOnlyList<CardData> cards) {
        if (cards == null || cards.Count != 5)
            return false;

        var values = GetSortedUniqueRankValues(cards);

        if (values.Count != 5)
            return false;

        if (IsConsecutive(values))
            return true;


        if (values.Contains((int)Rank.Ace)) {
            var aceLowValues = new List<int>(values);

            int aceIndex = aceLowValues.IndexOf((int)Rank.Ace);
            aceLowValues[aceIndex] = 1;
            aceLowValues.Sort();

            if (IsConsecutive(aceLowValues))
                return true;
        }

        return false;
    }

    private static bool IsConsecutive(IReadOnlyList<int> values) {
        for (int i = 1; i < values.Count; i++) {
            if (values[i] != values[i - 1] + 1)
                return false;
        }

        return true;
    }

    private static List<int> GetSortedUniqueRankValues(IReadOnlyList<CardData> cards) {
        var values = new HashSet<int>();

        foreach (var card in cards) {
            values.Add((int)card.Rank);
        }

        var sortedValues = new List<int>(values);
        sortedValues.Sort();

        return sortedValues;
    }
}
