using System.Collections.Generic;

public class GameStateReducer {
    public static GameState Reduce(GameState gameState, IGameAction gameAction) {
        if (gameAction is DrawCardAction) {
            return DrawCard(gameState);
        }
        return gameState;
    }

    private static GameState DrawCard(GameState state) {
        if (state.Deck.Count == 0) {
            return state;
        }
      
        var newDeck = new List<string>(state.Deck);
        var newHand = new List<string>(state.Hand);
        
        newHand.Add(newDeck[0]);
        newDeck.RemoveAt(0);
        return new GameState(newDeck, state.Deck, newHand);
    } 
}
