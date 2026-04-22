public sealed class PokerHandResult
{
    public PokerHandType HandType { get; }

    public PokerHandResult(PokerHandType handType)
    {
        HandType = handType;
    }
}