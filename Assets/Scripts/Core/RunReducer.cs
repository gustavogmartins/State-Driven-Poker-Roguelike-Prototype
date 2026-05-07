using System;
using System.Collections.Generic;

namespace Core {
    public static class RunReducer {
        private const int BaseHandsPerBlind = 4;
        private const int BaseDiscardsPerBlind = 3;

        public static RunState Reduce(RunState state, GameAction action) {
            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            if (action is StartNewRunAction startNewRunAction) {
                return RunState.CreateInitial(
                    initialHandCards: startNewRunAction.InitialHandCards,
                    runSeed: startNewRunAction.RunSeed);
            }

            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }

            return action switch {
                ContinueRoundEndAction continueRoundEnd => ContinueRoundEnd(state, continueRoundEnd.InitialHandCards),
                ToggleCardSelectionAction or
                    PlaySelectedCardsAction or
                    DiscardSelectedCardsAction or
                    SortHandByRankAction or
                    SortHandBySuitAction => ReduceRoundAction(state, action),
                ContinueShopAction continueShop => ContinueShop(state, continueShop.InitialHandCards),
                SelectShopOfferAction selectShopOffer => SelectShopOffer(state, selectShopOffer.Index),
                BuyShopOfferAction buyShopOffer => BuyShopOffer(state, buyShopOffer.Index),
                RerollShopAction => RerollShop(state),
                SelectOwnedJokerAction selectOwnedJoker => SelectOwnedJoker(state, selectOwnedJoker.Index),
                SellOwnedJokerAction sellOwnedJoker => SellOwnedJoker(state, sellOwnedJoker.Index),
                _ => state
            };
        }

        private static RunState ReduceRoundAction(RunState state, GameAction action) {
            if (state.IsInShop) {
                return state;
            }

            if (action is PlaySelectedCardsAction) {
                return PlaySelectedCards(state);
            }

            RoundState nextRound = RoundReducer.Reduce(state.CurrentRound, action);
            return ReferenceEquals(nextRound, state.CurrentRound)
                ? state
                : CopyWith(state, currentRound: nextRound);
        }

        private static RunState PlaySelectedCards(RunState state) {
            IReadOnlyList<CardData> selectedCards = state.CurrentRound.GetSelectedCards();
            PokerHandResult handResult = PokerHandEvaluator.Evaluate(selectedCards);
            ScoreResult baseScore = ScoreCalculator.Calculate(selectedCards, handResult, state.CurrentRound.Blind);
            JokerModifierResult modifierResult = RunModifierService.ApplyModifiers(baseScore, state.OwnedJokers, selectedCards, handResult);
            RoundState nextRound = RoundReducer.PlaySelectedCards(
                state.CurrentRound,
                modifierResult.ScoreResult,
                modifierResult.TriggeredText);

            if (ReferenceEquals(nextRound, state.CurrentRound)) {
                return state;
            }

            int nextMoney = state.Money + modifierResult.MoneyBonus;
            RunPhase nextPhase = nextRound.HasLostRound ? RunPhase.RunEnd : state.Phase;

            if (!state.CurrentRound.HasWonRound && nextRound.HasWonRound) {
                nextMoney += nextRound.BlindReward;
            }

            return new RunState(
                nextRound,
                null,
                state.OwnedJokers,
                nextMoney,
                nextPhase,
                state.ShopRefreshCount,
                state.RunSeed);
        }

        private static RunState ContinueRoundEnd(RunState state, IReadOnlyList<CardData> initialHandCards) {
            if (state.CanEnterShop) {
                BlindState nextBlind = state.CurrentBlind.Advance();
                ShopState shopState = new ShopState(
                    state.Money,
                    nextBlind,
                    ownedJokers: state.OwnedJokers,
                    offerPageIndex: state.ShopRefreshCount,
                    runSeed: state.RunSeed
                );

                return new RunState(
                    state.CurrentRound,
                    shopState,
                    state.OwnedJokers,
                    state.Money,
                    RunPhase.Shop,
                    state.ShopRefreshCount + 1,
                    state.RunSeed);
            }

            return state.IsRunOver || state.CurrentRound.HasLostRound
                ? RunState.CreateInitial(initialHandCards: initialHandCards)
                : state;
        }

        private static RunState ContinueShop(RunState state, IReadOnlyList<CardData> initialHandCards) {
            if (!state.IsInShop || state.CurrentShop == null) {
                return state;
            }

            RoundState nextRound = CreateRoundForBlind(
                state,
                state.CurrentShop.NextBlind,
                initialHandCards
            );

            return new RunState(
                nextRound,
                null,
                state.OwnedJokers,
                state.Money,
                RunPhase.Blind,
                state.ShopRefreshCount,
                state.RunSeed);
        }

        private static RunState SelectShopOffer(RunState state, int index) {
            if (!state.IsInShop || state.CurrentShop == null) {
                return state;
            }

            ShopState selectedShop = ShopReducer.SelectOffer(state.CurrentShop, index);
            return ReferenceEquals(selectedShop, state.CurrentShop)
                ? state
                : CopyWith(state, currentShop: selectedShop);
        }

        private static RunState BuyShopOffer(RunState state, int index) {
            if (!state.IsInShop || state.CurrentShop == null) {
                return state;
            }

            ShopState selectedShop = ShopReducer.SelectOffer(state.CurrentShop, index);
            if (ReferenceEquals(selectedShop, state.CurrentShop) && state.CurrentShop.SelectedOfferIndex != index) {
                return state;
            }

            RunState selectedState = ReferenceEquals(selectedShop, state.CurrentShop)
                ? state
                : CopyWith(state, currentShop: selectedShop);

            return BuySelectedShopOffer(selectedState);
        }

        private static RunState BuySelectedShopOffer(RunState state) {
            ShopOfferState selectedOffer = state.CurrentShop.SelectedOffer;
            if (selectedOffer == null || state.HasFullJokerInventory || !selectedOffer.CanBuy(state.Money)) {
                return state;
            }

            int updatedMoney = state.Money - selectedOffer.Cost;
            ShopState updatedShop = ShopReducer.PurchaseOffer(state.CurrentShop, selectedOffer.Id, updatedMoney);
            var updatedOwnedJokers = new List<JokerState>(state.OwnedJokers);
            if (!ContainsOwnedJoker(state.OwnedJokers, selectedOffer.Id)) {
                updatedOwnedJokers.Add(new JokerState(selectedOffer.Joker));
            }

            return new RunState(
                state.CurrentRound,
                updatedShop,
                updatedOwnedJokers,
                updatedMoney,
                state.Phase,
                state.ShopRefreshCount,
                state.RunSeed);
        }

        private static RunState RerollShop(RunState state) {
            if (!state.IsInShop || state.CurrentShop == null || !state.CurrentShop.CanReroll(state.Money)) {
                return state;
            }

            int updatedMoney = state.Money - state.CurrentShop.RerollCost;
            ShopState rerolledShop = ShopReducer.Reroll(state.CurrentShop, updatedMoney, state.OwnedJokers);
            return new RunState(
                state.CurrentRound,
                rerolledShop,
                state.OwnedJokers,
                updatedMoney,
                state.Phase,
                state.ShopRefreshCount + 1,
                state.RunSeed);
        }

        private static RunState SelectOwnedJoker(RunState state, int index) {
            if (!state.IsInShop || state.CurrentShop == null || !state.CanSellOwnedJoker(index)) {
                return state;
            }

            ShopState selectedShop = ShopReducer.SelectOwnedJoker(state.CurrentShop, index, state.OwnedJokers.Count);
            return ReferenceEquals(selectedShop, state.CurrentShop)
                ? state
                : CopyWith(state, currentShop: selectedShop);
        }

        private static RunState SellOwnedJoker(RunState state, int index) {
            if (!state.CanSellOwnedJoker(index)) {
                return state;
            }

            int updatedMoney = state.Money + state.GetOwnedJokerSellValue(index);
            var updatedOwnedJokers = new List<JokerState>(state.OwnedJokers);
            updatedOwnedJokers.RemoveAt(index);

            ShopState updatedShop = ShopReducer.SyncPurchasedOffers(state.CurrentShop, updatedOwnedJokers, updatedMoney);
            return new RunState(
                state.CurrentRound,
                updatedShop,
                updatedOwnedJokers,
                updatedMoney,
                state.Phase,
                state.ShopRefreshCount,
                state.RunSeed);
        }

        private static RunState CopyWith(
            RunState state,
            RoundState currentRound = null,
            ShopState currentShop = null,
            int? money = null,
            RunPhase? phase = null) {
            return new RunState(
                currentRound ?? state.CurrentRound,
                currentShop ?? state.CurrentShop,
                state.OwnedJokers,
                money ?? state.Money,
                phase ?? state.Phase,
                state.ShopRefreshCount,
                state.RunSeed
            );
        }

        private static RoundState CreateRoundForBlind(
            RunState state,
            BlindState blind,
            IReadOnlyList<CardData> initialHandCards = null) {
            return RoundState.CreateInitial(
                blind: blind,
                handsLeft: BaseHandsPerBlind + GetPassiveBonus(state.OwnedJokers, JokerBonusType.ExtraHand),
                discardsLeft: BaseDiscardsPerBlind + GetPassiveBonus(state.OwnedJokers, JokerBonusType.ExtraDiscard),
                maxHandSize: state.CurrentRound.MaxHandSize,
                initialHandCards: initialHandCards
            );
        }

        private static int GetPassiveBonus(IReadOnlyList<JokerState> ownedJokers, JokerBonusType bonusType) {
            int total = 0;

            for (int i = 0; i < ownedJokers.Count; i++) {
                JokerState joker = ownedJokers[i];
                if (joker.BonusType == bonusType && joker.ConditionType == JokerConditionType.Always) {
                    total += joker.BonusValue;
                }
            }

            return total;
        }

        private static bool ContainsOwnedJoker(IReadOnlyList<JokerState> ownedJokers, string jokerId) {
            for (int i = 0; i < ownedJokers.Count; i++) {
                if (ownedJokers[i].Id == jokerId) {
                    return true;
                }
            }

            return false;
        }
    }
}
