using System;
using System.Collections.Generic;

public static class DeckShuffler {
    public static List<CardData> Shuffle(IReadOnlyList<CardData> cards, int? seed = null) {
        var shuffled = new List<CardData>(cards);
        var random = seed.HasValue ? new Random(seed.Value) : new Random();

        for (int i = shuffled.Count - 1; i > 0; i--) {
            int j = random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        return shuffled;
    }
}