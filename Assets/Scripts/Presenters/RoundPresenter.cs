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
                HandsLeftText = $"Hands: {roundState.HandsLeft}",
                PhaseText = $"Phase: {roundState.Phase}",
            };

            foreach (var card in roundState.HandCards) {
                viewModel.HandCards.Add(new CardViewModel {
                    CardName = FormatCard(card)   
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
    }
}