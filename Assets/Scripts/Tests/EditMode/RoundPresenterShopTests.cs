using Core;
using NUnit.Framework;
using Presenters;
using View;

public sealed class RoundPresenterShopTests {
    [Test]
    public void Present_WhenRunIsInShop_CreatesThreeShopOfferViewModels() {
        RunState state = CreateShopRunState(money: 25);

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.ShopOffers, Has.Count.EqualTo(3));
        Assert.That(viewModel.ShopOffers[0].TitleText, Is.EqualTo("Glass Joker"));
        Assert.That(viewModel.ShopOffers[0].RarityText, Is.EqualTo("Common"));
        Assert.That(viewModel.ShopOffers[0].CostText, Is.EqualTo("$6"));
        Assert.That(viewModel.ShopOffers[0].StatusText, Is.EqualTo("Selected"));
        Assert.That(viewModel.ShopOffers[0].IsSelected, Is.True);
        Assert.That(viewModel.ShopOffers[0].CanBuy, Is.True);
        Assert.That(viewModel.ShopOffers[1].IsSelected, Is.False);
    }

    [Test]
    public void Present_WhenSelectedOfferIsBought_DisablesOfferAndShowsBoughtStatus() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) }
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.ShopOffers[0].IsPurchased, Is.True);
        Assert.That(viewModel.ShopOffers[0].CanBuy, Is.False);
        Assert.That(viewModel.ShopOffers[0].StatusText, Is.EqualTo("Bought"));
    }

    [Test]
    public void Present_WhenMoneyIsTooLow_ShowsNeedCostStatus() {
        RunState state = CreateShopRunState(money: 4);

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.ShopOffers[0].CanBuy, Is.False);
        Assert.That(viewModel.ShopOffers[0].StatusText, Is.EqualTo("Need $6"));
    }

    [Test]
    public void Present_WhenRerollIsAffordable_ShowsFiveDollarRerollCost() {
        RunState state = CreateShopRunState(money: 25);

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.CanRerollShop, Is.True);
        Assert.That(viewModel.ShopRerollButtonText, Is.EqualTo("Reroll ($5)"));
    }

    [Test]
    public void Present_WhenRerollIsNotAffordable_ShowsNeedFiveDollars() {
        RunState state = CreateShopRunState(money: 4);

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.CanRerollShop, Is.False);
        Assert.That(viewModel.ShopRerollButtonText, Is.EqualTo("Need $5"));
    }

    [Test]
    public void Present_WhenRunIsInShop_MarksOwnedJokersAsSellable() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) }
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.OwnedJokerCards, Has.Count.EqualTo(1));
        Assert.That(viewModel.OwnedJokerCards[0].Index, Is.EqualTo(0));
        Assert.That(viewModel.OwnedJokerCards[0].IsInteractable, Is.True);
        Assert.That(viewModel.OwnedJokerCards[0].CanSell, Is.True);
        Assert.That(viewModel.OwnedJokerCards[0].SellButtonText, Is.EqualTo("Sell $3"));
        Assert.That(viewModel.OwnedJokerCards[0].HasTooltip, Is.True);
        Assert.That(viewModel.OwnedJokerCards[0].TooltipTitleText, Is.EqualTo("Glass Joker"));
        Assert.That(viewModel.OwnedJokerCards[0].TooltipBodyText, Is.EqualTo("+10 Chips on every scoring hand"));
    }

    [Test]
    public void Present_WhenRunIsNotInShop_DoesNotMarkOwnedJokersAsSellable() {
        RunState state = new RunState(
            currentRound: RoundState.CreateInitial(),
            currentShop: null,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) },
            money: 25,
            phase: RunPhase.Blind
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.OwnedJokerCards, Has.Count.EqualTo(1));
        Assert.That(viewModel.OwnedJokerCards[0].Index, Is.EqualTo(0));
        Assert.That(viewModel.OwnedJokerCards[0].IsInteractable, Is.False);
        Assert.That(viewModel.OwnedJokerCards[0].CanSell, Is.False);
        Assert.That(viewModel.OwnedJokerCards[0].HasTooltip, Is.True);
    }

    [Test]
    public void Present_WhenRoundIsScoring_MarksTriggeredOwnedJokersForScoreAnimation() {
        var playedCards = new[] {
            new CardData(201, Rank.Two, Suit.Hearts),
            new CardData(202, Rank.Two, Suit.Spades)
        };
        RoundState round = RoundState.CreateInitial(initialHandCards: playedCards);
        round = RoundReducer.Reduce(round, new ToggleCardSelectionAction(0));
        round = RoundReducer.Reduce(round, new ToggleCardSelectionAction(1));
        var state = new RunState(
            currentRound: round,
            currentShop: null,
            ownedJokers: CreateOwnedJokers("glass-joker", "ace-tag"),
            money: 10,
            phase: RunPhase.Blind
        );
        RunState scoringState = RunReducer.Reduce(state, new PlaySelectedCardsAction());

        RoundViewModel viewModel = new RoundPresenter().Present(scoringState);

        Assert.That(viewModel.OwnedJokerCards[0].IsScoringJoker, Is.True);
        Assert.That(viewModel.OwnedJokerCards[0].ScoringJokerBonusType, Is.EqualTo(JokerBonusType.Chips));
        Assert.That(viewModel.OwnedJokerCards[0].ScoringJokerBonusValue, Is.EqualTo(10));
        Assert.That(viewModel.OwnedJokerCards[0].ScoringJokerPopupText, Is.EqualTo("+10"));
        Assert.That(viewModel.OwnedJokerCards[1].IsScoringJoker, Is.False);
    }

    [Test]
    public void Present_WhenInventoryIsFull_DisablesUnownedOffers() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: CreateOwnedJokers(
                "ace-tag",
                "pair-glove",
                "club-chip",
                "straight-polish",
                "heart-tag")
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.ShopOffers[0].CanBuy, Is.False);
        Assert.That(viewModel.ShopOffers[0].StatusText, Is.EqualTo("Inventory Full"));
        StringAssert.Contains("Inventory: 5/5 jokers", viewModel.ShopSummaryText);
    }

    [Test]
    public void Present_WhenRoundWasWon_UsesRoundEndBannerAsPrimaryAction() {
        RunState state = CreateRoundEndRunState(hasWonRound: true);

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.ShowRoundEndOverlay, Is.True);
        Assert.That(viewModel.RoundEndBannerText, Is.EqualTo("Go To Shop"));
        Assert.That(viewModel.RoundEndPrimaryActionText, Is.EqualTo("Go To Shop"));
        StringAssert.Contains("Blind cleared", viewModel.RoundEndSummaryText);
    }

    [Test]
    public void Present_WhenRoundWasLost_UsesRoundEndBannerAsPrimaryAction() {
        RunState state = CreateRoundEndRunState(hasWonRound: false);

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.ShowRoundEndOverlay, Is.True);
        Assert.That(viewModel.RoundEndBannerText, Is.EqualTo("New Run"));
        Assert.That(viewModel.RoundEndPrimaryActionText, Is.EqualTo("New Run"));
        StringAssert.Contains("No hands remaining", viewModel.RoundEndSummaryText);
    }

    private static RunState CreateShopRunState(int money, JokerState[] ownedJokers = null) {
        RoundState wonRound = new RoundState(
            blind: new BlindState(BlindType.Small, 1),
            targetScore: 300,
            currentScore: 300,
            handsLeft: 1,
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
        );

        return new RunState(
            currentRound: wonRound,
            currentShop: new ShopState(
                money,
                new BlindState(BlindType.Big, 1),
                offers: CreateFixedShopOffers(ownedJokers)),
            ownedJokers: ownedJokers ?? System.Array.Empty<JokerState>(),
            money: money,
            phase: RunPhase.Shop
        );
    }

    private static RunState CreateRoundEndRunState(bool hasWonRound) {
        RoundState round = new RoundState(
            blind: new BlindState(BlindType.Small, 1),
            targetScore: 300,
            currentScore: hasWonRound ? 300 : 120,
            handsLeft: hasWonRound ? 1 : 0,
            discardsLeft: 0,
            phase: RoundPhase.RoundEnd,
            maxHandSize: 5,
            deckCards: System.Array.Empty<CardData>(),
            handCards: System.Array.Empty<CardData>(),
            discardPileCards: System.Array.Empty<CardData>(),
            selectedCardsIndexes: System.Array.Empty<int>(),
            lastActionText: hasWonRound ? "Blind cleared" : "Round lost",
            lastPlayedCardsText: "None",
            lastPlayedCards: System.Array.Empty<CardData>(),
            lastPlayedCardsCount: 0,
            lastPlayedHandResult: PokerHandType.None,
            lastScoreResult: ScoreResult.Zero
        );

        return new RunState(
            currentRound: round,
            currentShop: null,
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 10,
            phase: hasWonRound ? RunPhase.Blind : RunPhase.RunEnd
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

    private static JokerState[] CreateOwnedJokers(params string[] jokerIds) {
        var ownedJokers = new JokerState[jokerIds.Length];

        for (int i = 0; i < jokerIds.Length; i++) {
            ownedJokers[i] = new JokerState(JokerCatalog.GetById(jokerIds[i]));
        }

        return ownedJokers;
    }
}
