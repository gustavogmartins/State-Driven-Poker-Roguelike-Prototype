public sealed class PokerHandResult
{
    public PokerHandType HandType { get; }
    public bool IsAceLowStraight { get; }
    
    public PokerHandResult(PokerHandType handType, bool isAceLowStraight = false)
    {
        HandType = handType;
        IsAceLowStraight = isAceLowStraight;
    }
}