public readonly struct HandBaseScore {
    public int Chips { get; }
    public int Mult { get; }

    public HandBaseScore(int chips, int mult) {
        Chips = chips;
        Mult = mult;
    }
}

public static class HandBaseScoreTable {
    public static HandBaseScore Get(PokerHandType handType) {
        return handType switch {
            PokerHandType.HighCard => new HandBaseScore(5, 1),
            PokerHandType.Pair => new HandBaseScore(10, 2),
            PokerHandType.TwoPair => new HandBaseScore(20, 2),
            PokerHandType.ThreeOfAKind => new HandBaseScore(30, 3),
            PokerHandType.Straight => new HandBaseScore(30, 4),
            PokerHandType.Flush => new HandBaseScore(35, 4),
            PokerHandType.FullHouse => new HandBaseScore(40, 4),
            PokerHandType.FourOfAKind => new HandBaseScore(60, 7),
            PokerHandType.StraightFlush => new HandBaseScore(100, 8),
            _ => new HandBaseScore(0, 0)
        };
    }
}