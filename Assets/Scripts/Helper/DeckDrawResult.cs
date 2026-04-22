using System.Collections.Generic;

public sealed class DeckDrawResult
{
    public List<CardData> RemainingDeck { get; }
    public List<CardData> DrawnCards { get; }

    public DeckDrawResult(List<CardData> remainingDeck, List<CardData> drawnCards)
    {
        RemainingDeck = remainingDeck;
        DrawnCards = drawnCards;
    }
}