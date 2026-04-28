using Core;
using UnityEngine;
using View;

namespace Presenters {
    public sealed class RoundPresenter {
        private const int TotalDeckSize = 52;

        public RoundViewModel Present(RunState runState) {
            RoundState roundState = runState.CurrentRound;
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
                BlindRewardText = $"${roundState.BlindReward}",
                RoundScoreText = roundState.CurrentScore.ToString(),
                HandNameText = handName,
                HandLevelText = handName == "Select Cards" ? string.Empty : "lvl.1",
                ChipsText = activeScore.TotalChips.ToString(),
                MultText = activeScore.BaseMult.ToString(),
                HandsLeftText = roundState.HandsLeft.ToString(),
                DiscardsLeftText = roundState.DiscardsLeft.ToString(),
                MoneyText = $"${runState.Money}",
                AnteText = roundState.Ante.ToString(),
                RoundText = roundState.RoundNumber.ToString(),
                PhaseText = FormatPhase(roundState.Phase),
                StatusText = BuildStatusText(roundState),
                SelectedCountText = $"{roundState.SelectedCardsCount}/5",
                DeckCountText = $"{roundState.DeckCards.Count}/{TotalDeckSize}",
                HandSizeText = $"{roundState.HandCards.Count}/{roundState.MaxHandSize}",
                TopDiscardText = FormatTopDiscard(roundState),
                ShowRoundEndOverlay = roundState.IsRoundOver && !runState.IsInShop,
                IsWinningRoundEnd = roundState.HasWonRound,
                RoundEndBannerText = BuildRoundEndBannerText(roundState),
                RoundEndSummaryText = BuildRoundEndSummaryText(roundState),
                RoundEndDetailsText = BuildRoundEndDetailsText(runState),
                RoundEndPrimaryActionText = BuildRoundEndPrimaryActionText(runState),
                ShowShopOverlay = runState.IsInShop,
                ShopBannerText = BuildShopBannerText(runState),
                ShopSummaryText = BuildShopSummaryText(runState),
                ShopDetailsText = BuildShopDetailsText(runState),
                ShopPrimaryActionText = BuildShopPrimaryActionText(runState),
                CanPlayHand = !runState.IsInShop && roundState.CanPlaySelectedCards,
                CanDiscard = !runState.IsInShop && roundState.CanDiscardSelectedCards,
                CanSort = !runState.IsInShop && roundState.CanSortHand
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
                "Boss Blind" => "Final blind of the ante\nClear it to advance",
                _ => "Beat the blind\nand keep the run alive"
            };
        }

        private static string BuildStatusText(RoundState roundState) {
            if (roundState.HasWonRound) {
                return $"Round End | Blind cleared. Reward: ${roundState.BlindReward}";
            }

            if (roundState.HasLostRound) {
                return "Round End | Round failed. No hands remaining";
            }

            if (roundState.LastActionText == "Waiting for input") {
                return $"{FormatPhase(roundState.Phase)} | {roundState.RemainingScore} score to clear";
            }

            return $"{FormatPhase(roundState.Phase)} | {roundState.LastActionText}";
        }

        private static string BuildRoundEndBannerText(RoundState roundState) {
            if (roundState.HasWonRound) {
                return $"Cash Out: ${roundState.BlindReward}";
            }

            if (roundState.HasLostRound) {
                return "Game Over";
            }

            return string.Empty;
        }

        private static string BuildRoundEndSummaryText(RoundState roundState) {
            if (roundState.HasWonRound) {
                return $"Blind cleared\nScored {roundState.CurrentScore} / {roundState.TargetScore}";
            }

            if (roundState.HasLostRound) {
                return $"You scored {roundState.CurrentScore} / {roundState.TargetScore}\nNo hands remaining";
            }

            return string.Empty;
        }

        private static string BuildRoundEndDetailsText(RunState runState) {
            RoundState roundState = runState.CurrentRound;

            if (roundState.HasWonRound) {
                BlindState nextBlind = roundState.Blind.Advance();
                string nextBlindLabel = $"Ante {nextBlind.Ante} | {nextBlind.Name}";

                return
                    $"Blind reward        ${roundState.BlindReward}\n" +
                    $"Next blind          {nextBlindLabel}\n" +
                    "Next stop           Shop\n" +
                    $"Money total         ${runState.Money}";
            }

            if (roundState.HasLostRound) {
                string lastHandText = roundState.LastPlayedHandResult == PokerHandType.None
                    ? "No final hand played"
                    : $"Final hand          {FormatHandType(roundState.LastPlayedHandResult)}";

                return
                    $"{lastHandText}\n" +
                    $"Money total         ${runState.Money}\n" +
                    "Start a new run or exit";
            }

            return string.Empty;
        }

        private static string BuildRoundEndPrimaryActionText(RunState runState) {
            RoundState roundState = runState.CurrentRound;

            if (roundState.HasWonRound) {
                return "Go To Shop";
            }

            if (roundState.HasLostRound) {
                return "New Run";
            }

            return string.Empty;
        }

        private static string BuildShopBannerText(RunState runState) {
            if (!runState.IsInShop || runState.CurrentShop == null) {
                return string.Empty;
            }

            return "Shop Open";
        }

        private static string BuildShopSummaryText(RunState runState) {
            if (!runState.IsInShop || runState.CurrentShop == null) {
                return string.Empty;
            }

            BlindState nextBlind = runState.CurrentShop.NextBlind;
            return $"Spend your cash or move on\nNext blind: {nextBlind.Name}";
        }

        private static string BuildShopDetailsText(RunState runState) {
            if (!runState.IsInShop || runState.CurrentShop == null) {
                return string.Empty;
            }

            BlindState nextBlind = runState.CurrentShop.NextBlind;
            return
                $"Money available     ${runState.Money}\n" +
                $"Pending blind        Ante {nextBlind.Ante} | {nextBlind.Name}\n" +
                "Shop stock           Coming next slice";
        }

        private static string BuildShopPrimaryActionText(RunState runState) {
            if (!runState.IsInShop || runState.CurrentShop == null) {
                return string.Empty;
            }

            BlindState nextBlind = runState.CurrentShop.NextBlind;
            return nextBlind.Type == BlindType.Small
                ? $"Start Ante {nextBlind.Ante}"
                : $"Play {nextBlind.Name}";
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
                Suit.Clubs => new Color32(9, 116, 203, 255),
                Suit.Spades => new Color32(52, 66, 72, 255),
                _ => Color.white
            };
        }
    }
}
