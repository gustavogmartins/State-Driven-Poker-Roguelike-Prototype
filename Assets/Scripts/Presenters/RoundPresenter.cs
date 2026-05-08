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
                ? ScoreCalculator.Calculate(selectedCards, activeHandResult, roundState.Blind)
                : roundState.LastScoreResult;

            string handName = hasPreviewSelection
                ? FormatHandType(activeHandResult.HandType)
                : hasPlayedCards
                    ? FormatHandType(roundState.LastPlayedHandResult)
                    : "Select Cards";

            if (!hasPreviewSelection && !hasPlayedCards) {
                activeScore = ScoreResult.Zero;
            }

            if (hasPreviewSelection) {
                activeScore = RunModifierService.ApplyScoreModifiers(activeScore, runState.OwnedJokers, selectedCards, activeHandResult);
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
                MultText = BuildMultText(activeScore),
                HandsLeftText = roundState.HandsLeft.ToString(),
                DiscardsLeftText = roundState.DiscardsLeft.ToString(),
                MoneyText = $"${runState.Money}",
                AnteText = roundState.Ante.ToString(),
                RoundText = roundState.RoundNumber.ToString(),
                PhaseText = FormatPhase(roundState.Phase),
                Phase = roundState.Phase,
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
                ShopRerollButtonText = BuildShopRerollButtonText(runState),
                CanRerollShop = runState.CanRerollShop,
                CanPlayHand = !runState.IsInShop && roundState.CanPlaySelectedCards,
                CanDiscard = !runState.IsInShop && roundState.CanDiscardSelectedCards,
                CanSort = !runState.IsInShop && roundState.CanSortHand
            };

            for (int i = 0; i < roundState.HandCards.Count; i++) {
                var card = roundState.HandCards[i];

                viewModel.HandCards.Add(CreateCardViewModel(
                    card,
                    index: i,
                    zone: CardZone.Hand,
                    isSelected: roundState.IsSelected(i),
                    isInteractable: !runState.IsInShop && roundState.Phase == RoundPhase.PlayerTurn,
                    blind: roundState.Blind
                ));
            }

            foreach (var card in roundState.PlayedCards) {
                viewModel.PlayedCards.Add(CreateCardViewModel(
                    card,
                    index: -1,
                    zone: CardZone.Played,
                    isSelected: false,
                    isInteractable: false,
                    blind: roundState.Blind
                ));
            }

            for (int i = 0; i < runState.OwnedJokers.Count; i++) {
                viewModel.OwnedJokerCards.Add(CreateJokerCardViewModel(
                    runState.OwnedJokers[i],
                    index: i,
                    canSell: runState.CanSellOwnedJoker(i),
                    sellValue: runState.GetOwnedJokerSellValue(i),
                    isSellSelected: runState.CurrentShop?.SelectedOwnedJokerIndex == i
                ));
            }

            AddShopOfferViewModels(viewModel, runState);

            return viewModel;
        }

        private static CardViewModel CreateCardViewModel(
            CardData card,
            int index,
            CardZone zone,
            bool isSelected,
            bool isInteractable,
            BlindState blind) {
            return new CardViewModel {
                CardId = card.InstanceId,
                Zone = zone,
                Index = index,
                RankText = FormatRank(card.Rank),
                SuitText = FormatSuit(card.Suit),
                AccentColor = GetSuitColor(card.Suit),
                IsSelected = isSelected,
                IsInteractable = isInteractable,
                IsDebuffed = IsDebuffedByBlind(card, blind)
            };
        }

        private static CardViewModel CreateJokerCardViewModel(
            JokerState joker,
            int index,
            bool canSell,
            int sellValue,
            bool isSellSelected) {
            return new CardViewModel {
                Index = index,
                RankText = joker.ShortCode,
                SuitText = "J",
                AccentColor = GetJokerColor(joker),
                IsSelected = false,
                IsInteractable = canSell,
                CanSell = canSell,
                IsSellSelected = isSellSelected,
                SellButtonText = sellValue > 0 ? $"Sell ${sellValue}" : "Sell"
            };
        }

        private static void AddShopOfferViewModels(RoundViewModel viewModel, RunState runState) {
            if (!runState.IsInShop || runState.CurrentShop == null) {
                return;
            }

            for (int i = 0; i < runState.CurrentShop.Offers.Count; i++) {
                ShopOfferState offer = runState.CurrentShop.Offers[i];
                bool isSelected = i == runState.CurrentShop.SelectedOfferIndex;

                bool canBuy = !runState.HasFullJokerInventory && offer.CanBuy(runState.Money);

                viewModel.ShopOffers.Add(new ShopOfferViewModel {
                    Index = i,
                    TitleText = offer.Title,
                    RarityText = FormatRarity(offer.Rarity),
                    DescriptionText = offer.Description,
                    CostText = $"${offer.Cost}",
                    StatusText = BuildShopOfferStatusText(offer, runState.Money, isSelected, runState.HasFullJokerInventory),
                    IsSelected = isSelected,
                    IsPurchased = offer.IsPurchased,
                    CanBuy = canBuy,
                    AccentColor = GetJokerColor(new JokerState(offer.Joker)),
                    RarityColor = GetRarityColor(offer.Rarity)
                });
            }
        }

        private static string BuildShopOfferStatusText(ShopOfferState offer, int money, bool isSelected, bool isInventoryFull) {
            if (offer.IsPurchased) {
                return "Bought";
            }

            if (isInventoryFull) {
                return "Inventory Full";
            }

            if (!offer.CanBuy(money)) {
                return $"Need ${offer.Cost}";
            }

            return isSelected ? "Selected" : "Available";
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
            return
                $"{runState.CurrentShop.Offers.Count} joker offers loaded\n" +
                $"Inventory: {runState.OwnedJokers.Count}/{RunState.MaxOwnedJokers} jokers\n" +
                $"Next blind: {nextBlind.Name}";
        }

        private static string BuildShopDetailsText(RunState runState) {
            if (!runState.IsInShop || runState.CurrentShop == null) {
                return string.Empty;
            }

            BlindState nextBlind = runState.CurrentShop.NextBlind;
            return
                $"Money available     ${runState.Money}\n" +
                $"Pending blind        Ante {nextBlind.Ante} | {nextBlind.Name}\n" +
                $"Reroll cost          ${runState.CurrentShop.RerollCost}";
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

        private static string BuildShopRerollButtonText(RunState runState) {
            if (!runState.IsInShop || runState.CurrentShop == null) {
                return string.Empty;
            }

            if (!runState.CurrentShop.CanReroll(runState.Money)) {
                return $"Need ${runState.CurrentShop.RerollCost}";
            }

            return $"Reroll (${runState.CurrentShop.RerollCost})";
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

        private static string FormatRarity(JokerRarity rarity) {
            return rarity switch {
                JokerRarity.Common => "Common",
                JokerRarity.Uncommon => "Uncommon",
                JokerRarity.Rare => "Rare",
                _ => rarity.ToString()
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

        private static Color GetJokerColor(JokerState joker) {
            return joker.BonusType switch {
                JokerBonusType.Chips => new Color32(244, 158, 27, 255),
                JokerBonusType.Mult => new Color32(40, 138, 91, 255),
                JokerBonusType.XMult => new Color32(171, 83, 219, 255),
                JokerBonusType.Money => new Color32(222, 181, 55, 255),
                JokerBonusType.ExtraHand => new Color32(74, 154, 224, 255),
                JokerBonusType.ExtraDiscard => new Color32(83, 173, 163, 255),
                _ => Color.white
            };
        }

        private static string BuildMultText(ScoreResult scoreResult) {
            return scoreResult.MultMultiplier > 1
                ? $"{scoreResult.BaseMult} x{scoreResult.MultMultiplier}"
                : scoreResult.BaseMult.ToString();
        }

        private static bool IsDebuffedByBlind(CardData card, BlindState blind) {
            return blind?.Type == BlindType.Boss && card.Suit == Suit.Clubs;
        }

        private static Color GetRarityColor(JokerRarity rarity) {
            return rarity switch {
                JokerRarity.Common => new Color32(166, 181, 184, 255),
                JokerRarity.Uncommon => new Color32(74, 154, 224, 255),
                JokerRarity.Rare => new Color32(210, 125, 233, 255),
                _ => Color.white
            };
        }
    }
}
