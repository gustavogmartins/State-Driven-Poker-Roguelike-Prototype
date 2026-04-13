public sealed class CardData {
    public Rank Rank { get; }
    public Suit Suit { get; }
    
    public CardData(Rank rank, Suit suit) {
        Rank = rank;
        Suit = suit;
    }
}
