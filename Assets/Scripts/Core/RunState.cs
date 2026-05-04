using System;
using System.Collections.Generic;

namespace Core {
    public sealed class RunState {
        public RoundState CurrentRound { get; }
        public ShopState CurrentShop { get; }
        public IReadOnlyList<JokerState> OwnedJokers { get; }
        public int Money { get; }
        public RunPhase Phase { get; }
        public int ShopRefreshCount { get; }
        public BlindState CurrentBlind => CurrentRound.Blind;
        public BlindState PendingBlind => CurrentShop?.NextBlind;
        public bool IsRunOver => Phase == RunPhase.RunEnd;
        public bool CanAdvanceToNextBlind => !IsRunOver && CurrentRound.HasWonRound;
        public bool CanEnterShop => Phase == RunPhase.Blind && CurrentRound.HasWonRound;
        public bool IsInShop => Phase == RunPhase.Shop;
        public bool CanRerollShop => IsInShop && CurrentShop?.CanReroll(Money) == true;

        public RunState(
            RoundState currentRound,
            ShopState currentShop,
            IReadOnlyList<JokerState> ownedJokers,
            int money,
            RunPhase phase,
            int shopRefreshCount = 0) {
            CurrentRound = currentRound ?? throw new ArgumentNullException(nameof(currentRound));
            CurrentShop = currentShop;
            OwnedJokers = new List<JokerState>(ownedJokers ?? Array.Empty<JokerState>()).AsReadOnly();

            if (money < 0) {
                throw new ArgumentOutOfRangeException(nameof(money));
            }

            Money = money;
            Phase = phase;
            ShopRefreshCount = Math.Max(0, shopRefreshCount);
        }

        public static RunState CreateInitial(
            BlindState blind = null,
            int startingMoney = 10,
            int handsLeft = 4,
            int discardsLeft = 3,
            int maxHandSize = 8,
            IReadOnlyList<CardData> initialHandCards = null) {
            var roundState = RoundState.CreateInitial(
                blind: blind,
                handsLeft: handsLeft,
                discardsLeft: discardsLeft,
                maxHandSize: maxHandSize,
                initialHandCards: initialHandCards
            );

            return new RunState(roundState, null, Array.Empty<JokerState>(), startingMoney, RunPhase.Blind);
        }

        public RunState ToggleCardSelection(int index) {
            return CopyWith(currentRound: CurrentRound.ToggleCardSelection(index));
        }

        public RunState PlaySelectedCards() {
            IReadOnlyList<CardData> selectedCards = CurrentRound.GetSelectedCards();
            PokerHandResult handResult = PokerHandEvaluator.Evaluate(selectedCards);
            ScoreResult baseScore = ScoreCalculator.Calculate(selectedCards, handResult, CurrentRound.Blind);
            ScoreResult modifiedScore = RunModifierService.ApplyScoreModifiers(baseScore, OwnedJokers, selectedCards, handResult);
            RoundState nextRound = CurrentRound.PlaySelectedCards(modifiedScore);
            int nextMoney = Money;
            RunPhase nextPhase = nextRound.HasLostRound ? RunPhase.RunEnd : Phase;

            if (!CurrentRound.HasWonRound && nextRound.HasWonRound) {
                nextMoney += nextRound.BlindReward;
            }

            return new RunState(nextRound, null, OwnedJokers, nextMoney, nextPhase, ShopRefreshCount);
        }

        public RunState DiscardCards() {
            return CopyWith(currentRound: CurrentRound.DiscardCards());
        }

        public RunState SortHandByRank() {
            return CopyWith(currentRound: CurrentRound.SortHandByRank());
        }

        public RunState SortHandBySuit() {
            return CopyWith(currentRound: CurrentRound.SortHandBySuit());
        }

        public RunState StartNextBlind(IReadOnlyList<CardData> initialHandCards = null) {
            if (!CanAdvanceToNextBlind) {
                return this;
            }

            BlindState nextBlind = CurrentBlind.Advance();
            RoundState nextRound = RoundState.CreateInitial(
                blind: nextBlind,
                maxHandSize: CurrentRound.MaxHandSize,
                initialHandCards: initialHandCards
            );

            return new RunState(nextRound, null, OwnedJokers, Money, RunPhase.Blind, ShopRefreshCount);
        }

        public RunState EnterShop() {
            if (!CanEnterShop) {
                return this;
            }

            BlindState nextBlind = CurrentBlind.Advance();
            ShopState shopState = new ShopState(
                Money,
                nextBlind,
                ownedJokers: OwnedJokers,
                offerPageIndex: ShopRefreshCount
            );
            return new RunState(CurrentRound, shopState, OwnedJokers, Money, RunPhase.Shop, ShopRefreshCount + 1);
        }

        public RunState LeaveShop(IReadOnlyList<CardData> initialHandCards = null) {
            if (!IsInShop || CurrentShop == null) {
                return this;
            }

            RoundState nextRound = RoundState.CreateInitial(
                blind: CurrentShop.NextBlind,
                maxHandSize: CurrentRound.MaxHandSize,
                initialHandCards: initialHandCards
            );

            return new RunState(nextRound, null, OwnedJokers, Money, RunPhase.Blind, ShopRefreshCount);
        }

        public RunState BuySelectedShopOffer() {
            if (!IsInShop || CurrentShop == null) {
                return this;
            }

            ShopOfferState selectedOffer = CurrentShop.SelectedOffer;
            if (selectedOffer == null || !selectedOffer.CanBuy(Money)) {
                return this;
            }

            int updatedMoney = Money - selectedOffer.Cost;
            ShopState updatedShop = CurrentShop.PurchaseOffer(selectedOffer.Id, updatedMoney);
            var updatedOwnedJokers = new List<JokerState>(OwnedJokers);
            if (!ContainsOwnedJoker(selectedOffer.Id)) {
                updatedOwnedJokers.Add(new JokerState(selectedOffer.Joker));
            }

            return new RunState(CurrentRound, updatedShop, updatedOwnedJokers, updatedMoney, Phase, ShopRefreshCount);
        }

        public RunState BuyShopOffer(int index) {
            RunState selectedState = SelectShopOffer(index);
            return ReferenceEquals(selectedState, this) && (CurrentShop == null || CurrentShop.SelectedOfferIndex != index)
                ? this
                : selectedState.BuySelectedShopOffer();
        }

        public RunState RerollShop() {
            if (!IsInShop || CurrentShop == null || !CurrentShop.CanReroll(Money)) {
                return this;
            }

            int updatedMoney = Money - CurrentShop.RerollCost;
            ShopState rerolledShop = CurrentShop.Reroll(updatedMoney, OwnedJokers);
            return new RunState(CurrentRound, rerolledShop, OwnedJokers, updatedMoney, Phase, ShopRefreshCount + 1);
        }

        public RunState SelectNextShopOffer() {
            if (!IsInShop || CurrentShop == null) {
                return this;
            }

            return new RunState(CurrentRound, CurrentShop.SelectNextOffer(), OwnedJokers, Money, Phase, ShopRefreshCount);
        }

        public RunState SelectPreviousShopOffer() {
            if (!IsInShop || CurrentShop == null) {
                return this;
            }

            return new RunState(CurrentRound, CurrentShop.SelectPreviousOffer(), OwnedJokers, Money, Phase, ShopRefreshCount);
        }

        public RunState SelectShopOffer(int index) {
            if (!IsInShop || CurrentShop == null) {
                return this;
            }

            ShopState selectedShop = CurrentShop.SelectOffer(index);
            return ReferenceEquals(selectedShop, CurrentShop)
                ? this
                : new RunState(CurrentRound, selectedShop, OwnedJokers, Money, Phase, ShopRefreshCount);
        }

        public bool CanSellOwnedJoker(int index) {
            return IsInShop && CurrentShop != null && index >= 0 && index < OwnedJokers.Count;
        }

        public int GetOwnedJokerSellValue(int index) {
            return CanSellOwnedJoker(index)
                ? Math.Max(1, OwnedJokers[index].Cost / 2)
                : 0;
        }

        public RunState SellOwnedJoker(int index) {
            if (!CanSellOwnedJoker(index)) {
                return this;
            }

            int updatedMoney = Money + GetOwnedJokerSellValue(index);
            var updatedOwnedJokers = new List<JokerState>(OwnedJokers);
            updatedOwnedJokers.RemoveAt(index);

            ShopState updatedShop = CurrentShop.SyncPurchasedOffers(updatedOwnedJokers, updatedMoney);
            return new RunState(CurrentRound, updatedShop, updatedOwnedJokers, updatedMoney, Phase, ShopRefreshCount);
        }

        private RunState CopyWith(
            RoundState currentRound = null,
            int? money = null,
            RunPhase? phase = null) {
            return new RunState(
                currentRound ?? CurrentRound,
                CurrentShop,
                OwnedJokers,
                money ?? Money,
                phase ?? Phase,
                ShopRefreshCount
            );
        }

        private bool ContainsOwnedJoker(string jokerId) {
            for (int i = 0; i < OwnedJokers.Count; i++) {
                if (OwnedJokers[i].Id == jokerId) {
                    return true;
                }
            }

            return false;
        }
    }
}
