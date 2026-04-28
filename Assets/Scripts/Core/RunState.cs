using System;
using System.Collections.Generic;

namespace Core {
    public sealed class RunState {
        public RoundState CurrentRound { get; }
        public ShopState CurrentShop { get; }
        public int Money { get; }
        public RunPhase Phase { get; }
        public BlindState CurrentBlind => CurrentRound.Blind;
        public BlindState PendingBlind => CurrentShop?.NextBlind;
        public bool IsRunOver => Phase == RunPhase.RunEnd;
        public bool CanAdvanceToNextBlind => !IsRunOver && CurrentRound.HasWonRound;
        public bool CanEnterShop => Phase == RunPhase.Blind && CurrentRound.HasWonRound;
        public bool IsInShop => Phase == RunPhase.Shop;

        public RunState(RoundState currentRound, ShopState currentShop, int money, RunPhase phase) {
            CurrentRound = currentRound ?? throw new ArgumentNullException(nameof(currentRound));
            CurrentShop = currentShop;

            if (money < 0) {
                throw new ArgumentOutOfRangeException(nameof(money));
            }

            Money = money;
            Phase = phase;
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

            return new RunState(roundState, null, startingMoney, RunPhase.Blind);
        }

        public RunState ToggleCardSelection(int index) {
            return CopyWith(currentRound: CurrentRound.ToggleCardSelection(index));
        }

        public RunState PlaySelectedCards() {
            RoundState nextRound = CurrentRound.PlaySelectedCards();
            int nextMoney = Money;
            RunPhase nextPhase = nextRound.HasLostRound ? RunPhase.RunEnd : Phase;

            if (!CurrentRound.HasWonRound && nextRound.HasWonRound) {
                nextMoney += nextRound.BlindReward;
            }

            return new RunState(nextRound, null, nextMoney, nextPhase);
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

            return new RunState(nextRound, null, Money, RunPhase.Blind);
        }

        public RunState EnterShop() {
            if (!CanEnterShop) {
                return this;
            }

            BlindState nextBlind = CurrentBlind.Advance();
            ShopState shopState = new ShopState(Money, nextBlind);
            return new RunState(CurrentRound, shopState, Money, RunPhase.Shop);
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

            return new RunState(nextRound, null, Money, RunPhase.Blind);
        }

        private RunState CopyWith(
            RoundState currentRound = null,
            int? money = null,
            RunPhase? phase = null) {
            return new RunState(
                currentRound ?? CurrentRound,
                CurrentShop,
                money ?? Money,
                phase ?? Phase
            );
        }
    }
}
