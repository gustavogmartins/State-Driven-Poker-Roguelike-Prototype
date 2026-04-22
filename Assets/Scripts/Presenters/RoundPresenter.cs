using Core;
using View;

namespace Presenters {
    public sealed class RoundPresenter {
        public RoundViewModel Present(RoundState roundState) {
            var viewModel = new RoundViewModel {
                BlindText = $"Blind: {roundState.BlindName}",
                TargetScoreText = $"Target: {roundState.TargetScore}",
                CurrentScoreText = $"Score: {roundState.CurrentScore}",
                DiscardsLeftText = $"Discards: {roundState.DiscardsLeft}",
                DeckCountText = $"Deck: {roundState.DeckCards.Count}",
                HandsLeftText = $"Hands: {roundState.HandsLeft}",
                PhaseText = $"Phase: {roundState.Phase}",
                LastPlayedCountText = $"Played Count: {roundState.LastPlayedCardsCount}",
                LastPlayedCardsText = $"Last Played: {roundState.LastPlayedCardsText}",
                HandSizeText = $"{roundState.HandCards.Count}/{roundState.MaxHandSize}",
                DiscardPileCountText = $"Discard Pile: {roundState.DiscardPileCards.Count}",
                TopDiscardText = $"Top Discard: {FormatTopDiscard(roundState)}",
            };

            for (int i = 0; i < roundState.HandCards.Count; i++) {
                var card = roundState.HandCards[i];

                viewModel.HandCards.Add(new CardViewModel {
                    Index = i,
                    CardName = FormatCard(card),
                    IsSelected = roundState.IsSelected(i)
                });
            }

            return viewModel;
        }

        private string FormatCard(CardData card) {
            return $"{FormatRank(card.Rank)}{FormatSuit(card.Suit)}";
        }

        private string FormatSuit(Suit cardSuit) {
            return cardSuit switch {
                Suit.Clubs => "♣",
                Suit.Diamonds => "♦",
                Suit.Hearts => "♥",
                Suit.Spades => "♠",
                _ => "?"
            };
        }

        private string FormatRank(Rank cardRank) {
            return cardRank switch {
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                Rank.Ace => "A",
                Rank.Ten => "10",
                _ => ((int)cardRank).ToString()
            };
        }
        
        private string FormatTopDiscard(RoundState roundState)
        {
            if (roundState.DiscardPileCards.Count == 0)
                return "None";

            var topCard = roundState.DiscardPileCards[roundState.DiscardPileCards.Count - 1];
            return FormatCard(topCard);
        }
    }
}