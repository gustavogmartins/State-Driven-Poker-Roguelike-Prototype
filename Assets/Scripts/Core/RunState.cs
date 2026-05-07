using System;
using System.Collections.Generic;

namespace Core {
    public sealed class RunState {
        public const int MaxOwnedJokers = 5;

        public RoundState CurrentRound { get; }
        public ShopState CurrentShop { get; }
        public IReadOnlyList<JokerState> OwnedJokers { get; }
        public int Money { get; }
        public RunPhase Phase { get; }
        public int ShopRefreshCount { get; }
        public int RunSeed { get; }
        public BlindState CurrentBlind => CurrentRound.Blind;
        public BlindState PendingBlind => CurrentShop?.NextBlind;
        public bool IsRunOver => Phase == RunPhase.RunEnd;
        public bool CanAdvanceToNextBlind => !IsRunOver && CurrentRound.HasWonRound;
        public bool CanEnterShop => Phase == RunPhase.Blind && CurrentRound.HasWonRound;
        public bool IsInShop => Phase == RunPhase.Shop;
        public bool CanRerollShop => IsInShop && CurrentShop?.CanReroll(Money) == true;
        public bool HasFullJokerInventory => OwnedJokers.Count >= MaxOwnedJokers;

        public RunState(
            RoundState currentRound,
            ShopState currentShop,
            IReadOnlyList<JokerState> ownedJokers,
            int money,
            RunPhase phase,
            int shopRefreshCount = 0,
            int runSeed = 0) {
            CurrentRound = currentRound ?? throw new ArgumentNullException(nameof(currentRound));
            CurrentShop = currentShop;
            OwnedJokers = new List<JokerState>(ownedJokers ?? Array.Empty<JokerState>()).AsReadOnly();

            if (money < 0) {
                throw new ArgumentOutOfRangeException(nameof(money));
            }

            Money = money;
            Phase = phase;
            ShopRefreshCount = Math.Max(0, shopRefreshCount);
            RunSeed = runSeed;
        }

        public static RunState CreateInitial(
            BlindState blind = null,
            int startingMoney = 10,
            int handsLeft = 4,
            int discardsLeft = 3,
            int maxHandSize = 8,
            IReadOnlyList<CardData> initialHandCards = null,
            int? runSeed = null) {
            var roundState = RoundState.CreateInitial(
                blind: blind,
                handsLeft: handsLeft,
                discardsLeft: discardsLeft,
                maxHandSize: maxHandSize,
                initialHandCards: initialHandCards
            );

            return new RunState(
                roundState,
                null,
                Array.Empty<JokerState>(),
                startingMoney,
                RunPhase.Blind,
                runSeed: runSeed ?? Environment.TickCount);
        }

        public bool CanSellOwnedJoker(int index) {
            return IsInShop && CurrentShop != null && index >= 0 && index < OwnedJokers.Count;
        }

        public int GetOwnedJokerSellValue(int index) {
            return CanSellOwnedJoker(index)
                ? Math.Max(1, OwnedJokers[index].Cost / 2)
                : 0;
        }
    }
}
