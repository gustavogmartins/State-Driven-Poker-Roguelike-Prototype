using Core;
using NUnit.Framework;
using Presenters;
using View;

public sealed class GameStoreTests {
    [Test]
    public void Dispatch_WhenActionDoesNotChangeState_DoesNotRaiseStateChanged() {
        RunState state = RunState.CreateInitial(runSeed: 123);
        var store = new GameStore(state);
        int eventCount = 0;
        store.StateChanged += _ => eventCount++;

        RunState nextState = store.Dispatch(new SelectShopOfferAction(0));

        Assert.That(nextState, Is.SameAs(state));
        Assert.That(store.State, Is.SameAs(state));
        Assert.That(eventCount, Is.EqualTo(0));
    }

    [Test]
    public void Dispatch_WhenRoundIsLostAndContinueRoundEnd_StartsNewRun() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Two, Suit.Spades)
        };

        var store = new GameStore(RunState.CreateInitial(
            blind: new BlindState(BlindType.Small, 1),
            handsLeft: 1,
            maxHandSize: 1,
            initialHandCards: handCards,
            runSeed: 123
        ));

        store.Dispatch(new ToggleCardSelectionAction(0));
        store.Dispatch(new PlaySelectedCardsAction());
        RunState lostState = store.State;

        store.Dispatch(new ContinueRoundEndAction(handCards));

        Assert.That(lostState.IsRunOver, Is.True);
        Assert.That(store.State, Is.Not.SameAs(lostState));
        Assert.That(store.State.Phase, Is.EqualTo(RunPhase.Blind));
        Assert.That(store.State.CurrentRound.Phase, Is.EqualTo(RoundPhase.PlayerTurn));
    }

    [Test]
    public void Dispatch_WhenOwnedJokerIsSelected_PresenterMarksSellSelection() {
        var store = new GameStore(CreateShopRunState(
            money: 25,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) }
        ));

        store.Dispatch(new SelectOwnedJokerAction(0));

        RoundViewModel viewModel = new RoundPresenter().Present(store.State);

        Assert.That(store.State.CurrentShop.SelectedOwnedJokerIndex, Is.EqualTo(0));
        Assert.That(viewModel.OwnedJokerCards[0].IsSellSelected, Is.True);
    }

    [Test]
    public void Dispatch_WhenSelectedOwnedJokerIsSold_ClearsSellSelection() {
        var store = new GameStore(CreateShopRunState(
            money: 25,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) }
        ));

        store.Dispatch(new SelectOwnedJokerAction(0));
        store.Dispatch(new SellOwnedJokerAction(0));

        Assert.That(store.State.OwnedJokers, Has.Count.EqualTo(0));
        Assert.That(store.State.CurrentShop.SelectedOwnedJokerIndex, Is.EqualTo(-1));
    }

    private static RunState CreateShopRunState(int money, JokerState[] ownedJokers = null) {
        return new RunState(
            currentRound: new RoundState(
                blind: new BlindState(BlindType.Small, 1),
                targetScore: 300,
                currentScore: 300,
                handsLeft: 0,
                discardsLeft: 3,
                phase: RoundPhase.RoundEnd,
                maxHandSize: 5,
                deckCards: System.Array.Empty<CardData>(),
                handCards: System.Array.Empty<CardData>(),
                discardPileCards: System.Array.Empty<CardData>(),
                selectedCardsIndexes: System.Array.Empty<int>(),
                lastActionText: "Blind cleared",
                lastPlayedCardsText: "None",
                lastPlayedCards: System.Array.Empty<CardData>(),
                lastPlayedCardsCount: 0,
                lastPlayedHandResult: PokerHandType.None,
                lastScoreResult: ScoreResult.Zero
            ),
            currentShop: new ShopState(
                money,
                new BlindState(BlindType.Big, 1),
                offers: CreateFixedShopOffers(ownedJokers)),
            ownedJokers: ownedJokers ?? System.Array.Empty<JokerState>(),
            money: money,
            phase: RunPhase.Shop
        );
    }

    private static ShopOfferState[] CreateFixedShopOffers(JokerState[] ownedJokers = null) {
        return new[] {
            new ShopOfferState(
                JokerCatalog.GetById("glass-joker"),
                IsOwned(ownedJokers, "glass-joker")),
            new ShopOfferState(
                JokerCatalog.GetById("ace-tag"),
                IsOwned(ownedJokers, "ace-tag")),
            new ShopOfferState(
                JokerCatalog.GetById("pair-glove"),
                IsOwned(ownedJokers, "pair-glove"))
        };
    }

    private static bool IsOwned(JokerState[] ownedJokers, string jokerId) {
        if (ownedJokers == null) {
            return false;
        }

        for (int i = 0; i < ownedJokers.Length; i++) {
            if (ownedJokers[i].Id == jokerId) {
                return true;
            }
        }

        return false;
    }
}
