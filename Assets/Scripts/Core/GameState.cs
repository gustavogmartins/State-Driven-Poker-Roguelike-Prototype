using System.Collections.Generic;

public class GameState {
    public List<string> Cards { get; }
    public List<string> Deck { get; }
    public List<string> Hand { get; }

    public GameState(List<string> cards, List<string> deck, List<string> hand) {
        Cards = cards;
        Deck = deck;
        Hand = hand;
    }

    public static GameState InitialState() {
        return new GameState(
            new List<string>(), 
            new List<string> {"A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"}, 
            new List<string>()
            );
    }
}