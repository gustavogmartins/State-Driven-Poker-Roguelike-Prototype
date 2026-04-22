using System.Collections.Generic;

public static class DeckUtility
{
    public static DeckDrawResult DrawCards(IReadOnlyList<CardData> deckCards, int amount)
    {
        var remainingDeck = new List<CardData>(deckCards);
        var drawnCards = new List<CardData>();

        for (int i = 0; i < amount; i++)
        {
            if (remainingDeck.Count == 0)
                break;

            var topCard = remainingDeck[0];
            remainingDeck.RemoveAt(0);
            drawnCards.Add(topCard);
        }

        return new DeckDrawResult(remainingDeck, drawnCards);
    }
}