using System.Collections.Generic;

public static class DeckBuilder {
    public static List<CardData> CreateStandard52() {
        var deck = new List<CardData>();

        foreach (Suit suit in System.Enum.GetValues(typeof(Suit))) {
            foreach (Rank rank in System.Enum.GetValues(typeof(Rank))) {
                deck.Add(new CardData(rank, suit));
            }
        }

        return deck;
    }
}