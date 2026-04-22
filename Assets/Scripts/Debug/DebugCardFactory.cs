using System.Collections.Generic;

public static class DebugCardFactory {
    private static readonly CardData[] CardsPool = {
        new CardData(Rank.Ace, Suit.Spades),
        new CardData(Rank.King, Suit.Hearts),
        new CardData(Rank.Queen, Suit.Diamonds),
        new CardData(Rank.Jack, Suit.Clubs),
        new CardData(Rank.Ten, Suit.Spades),
        new CardData(Rank.Nine, Suit.Hearts),
        new CardData(Rank.Eight, Suit.Diamonds),
        new CardData(Rank.Seven, Suit.Clubs),
        new CardData(Rank.Six, Suit.Spades),
        new CardData(Rank.Five, Suit.Hearts),
        new CardData(Rank.Four, Suit.Diamonds),
        new CardData(Rank.Three, Suit.Clubs),
        new CardData(Rank.Two, Suit.Spades)
    };

    private static int _nextIndex;

    private static CardData CreateNext() {
        var card = CardsPool[_nextIndex];
        _nextIndex = (_nextIndex + 1) % CardsPool.Length;
        return card;
    }

    public static List<CardData> FillToSize(IReadOnlyList<CardData> currentCards, int targetSize) {
        var result = new List<CardData>(currentCards);

        while (result.Count < targetSize) {
            result.Add(CreateNext());
        }

        return result;
    }
}