using Core;
using UnityEngine;
using View;

namespace Presenters {
    public sealed class RoundPresenter {
        private const int TotalDeckSize = 52;

        public RoundViewModel Present(RoundState roundState) {
            var selectedCards = roundState.GetSelectedCards();
            bool hasPreviewSelection = selectedCards.Count > 0;
            bool hasPlayedCards = roundState.LastPlayedCards.Count > 0;

            PokerHandResult activeHandResult = hasPreviewSelection
                ? PokerHandEvaluator.Evaluate(selectedCards)
                : new PokerHandResult(roundState.LastPlayedHandResult);

            ScoreResult activeScore = hasPreviewSelection
                ? ScoreCalculator.Calculate(selectedCards, activeHandResult)
                : roundState.LastScoreResult;

            string handName = hasPreviewSelection
                ? FormatHandType(activeHandResult.HandType)
                : hasPlayedCards
                    ? FormatHandType(roundState.LastPlayedHandResult)
                    : "Select Cards";

            if (!hasPreviewSelection && !hasPlayedCards) {
                activeScore = ScoreResult.Zero;
            }

            var viewModel = new RoundViewModel {
                BlindTitleText = roundState.BlindName,
                BlindDescriptionText = BuildBlindDescription(roundState.BlindName),
                BlindRequirementText = roundState.TargetScore.ToString(),
                BlindRewardText = $"${roundState.Ante * 10}",
                RoundScoreText = roundState.CurrentScore.ToString(),
                HandNameText = handName,
                HandLevelText = handName == "Select Cards" ? string.Empty : "lvl.1",
                ChipsText = activeScore.TotalChips.ToString(),
                MultText = activeScore.BaseMult.ToString(),
                HandsLeftText = roundState.HandsLeft.ToString(),
                DiscardsLeftText = roundState.DiscardsLeft.ToString(),
                MoneyText = $"${roundState.Money}",
                AnteText = roundState.Ante.ToString(),
                RoundText = roundState.RoundNumber.ToString(),
                PhaseText = FormatPhase(roundState.Phase),
                StatusText = BuildStatusText(roundState),
                SelectedCountText = $"{roundState.SelectedCardsCount}/5",
                DeckCountText = $"{roundState.DeckCards.Count}/{TotalDeckSize}",
                HandSizeText = $"{roundState.HandCards.Count}/{roundState.MaxHandSize}",
                TopDiscardText = FormatTopDiscard(roundState),
                CanPlayHand = roundState.CanPlaySelectedCards,
                CanDiscard = roundState.CanDiscardSelectedCards,
                CanSort = roundState.CanSortHand
            };

            for (int i = 0; i < roundState.HandCards.Count; i++) {
                var card = roundState.HandCards[i];

                viewModel.HandCards.Add(CreateCardViewModel(
                    card,
                    index: i,
                    isSelected: roundState.IsSelected(i),
                    isInteractable: roundState.Phase != RoundPhase.RoundEnd
                ));
            }

            foreach (var card in roundState.LastPlayedCards) {
                viewModel.PlayedCards.Add(CreateCardViewModel(
                    card,
                    index: -1,
                    isSelected: false,
                    isInteractable: false
                ));
            }

            return viewModel;
        }

        private static CardViewModel CreateCardViewModel(
            CardData card,
            int index,
            bool isSelected,
            bool isInteractable) {
            return new CardViewModel {
                Index = index,
                RankText = FormatRank(card.Rank),
                SuitText = FormatSuit(card.Suit),
                AccentColor = GetSuitColor(card.Suit),
                IsSelected = isSelected,
                IsInteractable = isInteractable
            };
        }

        private static string BuildBlindDescription(string blindName) {
            return blindName switch {
                "The Club" => "All Club cards\nare debuffed",
                "Small Blind" => "Opening blind\nNo debuffs active",
                "Big Blind" => "Higher stakes\nBeat the target cleanly",
                _ => "Beat the blind\nand keep the run alive"
            };
        }

        private static string BuildStatusText(RoundState roundState) {
            if (roundState.LastActionText == "Waiting for input") {
                return $"Phase: {FormatPhase(roundState.Phase)}";
            }

            return $"{FormatPhase(roundState.Phase)} | {roundState.LastActionText}";
        }

        private static string FormatTopDiscard(RoundState roundState) {
            if (roundState.DiscardPileCards.Count == 0) {
                return "Top discard: Empty";
            }

            var topCard = roundState.DiscardPileCards[roundState.DiscardPileCards.Count - 1];
            return $"Top discard: {FormatRank(topCard.Rank)}{FormatSuit(topCard.Suit)}";
        }

        private static string FormatPhase(RoundPhase phase) {
            return phase switch {
                RoundPhase.PlayerTurn => "Player Turn",
                RoundPhase.Scoring => "Scoring",
                RoundPhase.RoundEnd => "Round End",
                _ => "Waiting"
            };
        }

        private static string FormatHandType(PokerHandType handType) {
            return handType switch {
                PokerHandType.None => "No Hand",
                PokerHandType.HighCard => "High Card",
                PokerHandType.Pair => "Pair",
                PokerHandType.TwoPair => "Two Pair",
                PokerHandType.ThreeOfAKind => "Three of a Kind",
                PokerHandType.Straight => "Straight",
                PokerHandType.Flush => "Flush",
                PokerHandType.FullHouse => "Full House",
                PokerHandType.FourOfAKind => "Four of a Kind",
                PokerHandType.StraightFlush => "Straight Flush",
                _ => handType.ToString()
            };
        }

        private static string FormatRank(Rank rank) {
            return rank switch {
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                Rank.Ace => "A",
                Rank.Ten => "10",
                _ => ((int)rank).ToString()
            };
        }

        private static string FormatSuit(Suit suit) {
            return suit switch {
                Suit.Clubs => "\u2663",
                Suit.Diamonds => "\u2666",
                Suit.Hearts => "\u2665",
                Suit.Spades => "\u2660",
                _ => "?"
            };
        }

        private static Color GetSuitColor(Suit suit) {
            return suit switch {
                Suit.Hearts => new Color32(220, 53, 69, 255),
                Suit.Diamonds => new Color32(230, 153, 25, 255),
                _ => new Color32(52, 66, 72, 255)
            };
        }
    }
}
