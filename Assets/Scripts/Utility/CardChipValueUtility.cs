public static class CardChipValueUtility {
    public static int GetChipValue(CardData card) {
        return card.Rank switch {
            Rank.Ace => 11,
            Rank.King => 10,
            Rank.Queen => 10,
            Rank.Jack => 10,
            Rank.Ten => 10,
            _ => (int)card.Rank
        };
    }
}