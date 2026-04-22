using System.Collections.Generic;

public static class DebugHandFactory {
    public static List<CardData> Create(DebugHandScenario scenario) {
        return scenario switch {
            DebugHandScenario.Pair => new List<CardData> {
                new CardData(Rank.Ace, Suit.Spades),
                new CardData(Rank.Ace, Suit.Hearts),
                new CardData(Rank.Seven, Suit.Clubs),
                new CardData(Rank.Four, Suit.Diamonds),
                new CardData(Rank.Two, Suit.Spades),
                new CardData(Rank.King, Suit.Hearts),
                new CardData(Rank.Nine, Suit.Clubs),
                new CardData(Rank.Three, Suit.Diamonds),
            },

            DebugHandScenario.TwoPair => new List<CardData> {
                new CardData(Rank.Ace, Suit.Spades),
                new CardData(Rank.Ace, Suit.Hearts),
                new CardData(Rank.Seven, Suit.Clubs),
                new CardData(Rank.Seven, Suit.Diamonds),
                new CardData(Rank.Two, Suit.Spades),
                new CardData(Rank.King, Suit.Hearts),
                new CardData(Rank.Nine, Suit.Clubs),
                new CardData(Rank.Three, Suit.Diamonds),
            },

            DebugHandScenario.ThreeOfAKind => new List<CardData> {
                new CardData(Rank.King, Suit.Spades),
                new CardData(Rank.King, Suit.Hearts),
                new CardData(Rank.King, Suit.Diamonds),
                new CardData(Rank.Four, Suit.Clubs),
                new CardData(Rank.Two, Suit.Spades),
                new CardData(Rank.Queen, Suit.Hearts),
                new CardData(Rank.Nine, Suit.Clubs),
                new CardData(Rank.Three, Suit.Diamonds),
            },

            DebugHandScenario.FullHouse => new List<CardData> {
                new CardData(Rank.Queen, Suit.Spades),
                new CardData(Rank.Queen, Suit.Hearts),
                new CardData(Rank.Queen, Suit.Diamonds),
                new CardData(Rank.Eight, Suit.Clubs),
                new CardData(Rank.Eight, Suit.Diamonds),
                new CardData(Rank.King, Suit.Hearts),
                new CardData(Rank.Nine, Suit.Clubs),
                new CardData(Rank.Three, Suit.Diamonds),
            },

            DebugHandScenario.FourOfAKind => new List<CardData> {
                new CardData(Rank.Ten, Suit.Spades),
                new CardData(Rank.Ten, Suit.Hearts),
                new CardData(Rank.Ten, Suit.Diamonds),
                new CardData(Rank.Ten, Suit.Clubs),
                new CardData(Rank.Two, Suit.Spades),
                new CardData(Rank.King, Suit.Hearts),
                new CardData(Rank.Nine, Suit.Clubs),
                new CardData(Rank.Three, Suit.Diamonds),
            },

            DebugHandScenario.Straight => new List<CardData> {
                new CardData(Rank.Five, Suit.Spades),
                new CardData(Rank.Six, Suit.Hearts),
                new CardData(Rank.Seven, Suit.Clubs),
                new CardData(Rank.Eight, Suit.Diamonds),
                new CardData(Rank.Nine, Suit.Spades),
                new CardData(Rank.King, Suit.Hearts),
                new CardData(Rank.Two, Suit.Clubs),
                new CardData(Rank.Three, Suit.Diamonds),
            },

            DebugHandScenario.Flush => new List<CardData> {
                new CardData(Rank.Ace, Suit.Hearts),
                new CardData(Rank.Ten, Suit.Hearts),
                new CardData(Rank.Seven, Suit.Hearts),
                new CardData(Rank.Four, Suit.Hearts),
                new CardData(Rank.Two, Suit.Hearts),
                new CardData(Rank.King, Suit.Spades),
                new CardData(Rank.Nine, Suit.Clubs),
                new CardData(Rank.Three, Suit.Diamonds),
            },

            DebugHandScenario.StraightFlush => new List<CardData> {
                new CardData(Rank.Five, Suit.Clubs),
                new CardData(Rank.Six, Suit.Clubs),
                new CardData(Rank.Seven, Suit.Clubs),
                new CardData(Rank.Eight, Suit.Clubs),
                new CardData(Rank.Nine, Suit.Clubs),
                new CardData(Rank.King, Suit.Hearts),
                new CardData(Rank.Two, Suit.Spades),
                new CardData(Rank.Three, Suit.Diamonds),
            },

            _ => new List<CardData>()
        };
    }
}