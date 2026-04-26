internal static class TestCardFactory {
    public static CardData Create(Rank rank, Suit suit) {
        return new CardData(rank, suit);
    }
}
