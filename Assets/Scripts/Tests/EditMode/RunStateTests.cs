using Core;
using NUnit.Framework;

public sealed class RunStateTests {
    private const int TestRunSeed = 12345;

    [Test]
    public void PlaySelectedCards_WhenBlindIsCleared_AwardsMoneyToRun() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Five, Suit.Clubs),
            TestCardFactory.Create(Rank.Six, Suit.Clubs),
            TestCardFactory.Create(Rank.Seven, Suit.Clubs),
            TestCardFactory.Create(Rank.Eight, Suit.Clubs),
            TestCardFactory.Create(Rank.Nine, Suit.Clubs)
        };

        RunState state = RunState.CreateInitial(
            blind: new BlindState(BlindType.Small, 2),
            startingMoney: 10,
            handsLeft: 2,
            maxHandSize: 5,
            initialHandCards: handCards
        );

        for (int i = 0; i < 5; i++) {
            state = RunReducer.Reduce(state, new ToggleCardSelectionAction(i));
        }

        RunState scoringState = RunReducer.Reduce(state, new PlaySelectedCardsAction());

        Assert.That(scoringState.CurrentRound.Phase, Is.EqualTo(RoundPhase.Scoring));
        Assert.That(scoringState.CurrentRound.HasWonRound, Is.False);
        Assert.That(scoringState.Money, Is.EqualTo(10));
        Assert.That(scoringState.Phase, Is.EqualTo(RunPhase.Blind));

        RunState nextState = RunReducer.Reduce(scoringState, new ScorePresentationFinishedAction());

        Assert.That(nextState.CurrentRound.HasWonRound, Is.True);
        Assert.That(nextState.Money, Is.EqualTo(30));
        Assert.That(nextState.Phase, Is.EqualTo(RunPhase.Blind));
    }

    [Test]
    public void PlaySelectedCards_WhenRunIsLost_EndsRun() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Two, Suit.Spades)
        };

        RunState state = RunState.CreateInitial(
            blind: new BlindState(BlindType.Small, 1),
            handsLeft: 1,
            maxHandSize: 1,
            initialHandCards: handCards
        );

        state = RunReducer.Reduce(state, new ToggleCardSelectionAction(0));

        RunState scoringState = RunReducer.Reduce(state, new PlaySelectedCardsAction());

        Assert.That(scoringState.CurrentRound.Phase, Is.EqualTo(RoundPhase.Scoring));
        Assert.That(scoringState.CurrentRound.HasLostRound, Is.False);
        Assert.That(scoringState.IsRunOver, Is.False);

        RunState nextState = RunReducer.Reduce(scoringState, new ScorePresentationFinishedAction());

        Assert.That(nextState.CurrentRound.HasLostRound, Is.True);
        Assert.That(nextState.IsRunOver, Is.True);
        Assert.That(nextState.Phase, Is.EqualTo(RunPhase.RunEnd));
    }

    [Test]
    public void ContinueRoundEndAndShop_WhenRoundWasWon_AdvancesBlindAndKeepsMoney() {
        RunState state = new RunState(
            currentRound: new RoundState(
                blind: new BlindState(BlindType.Small, 1),
                targetScore: 300,
                currentScore: 300,
                handsLeft: 0,
                discardsLeft: 3,
                phase: RoundPhase.RoundEnd,
                maxHandSize: 1,
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
            currentShop: null,
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Blind
        );

        RunState shopState = RunReducer.Reduce(state, new ContinueRoundEndAction());
        RunState nextState = RunReducer.Reduce(shopState, new ContinueShopAction());

        Assert.That(nextState.CurrentBlind.Type, Is.EqualTo(BlindType.Big));
        Assert.That(nextState.CurrentRound.Ante, Is.EqualTo(1));
        Assert.That(nextState.CurrentRound.RoundNumber, Is.EqualTo(2));
        Assert.That(nextState.CurrentRound.TargetScore, Is.EqualTo(450));
        Assert.That(nextState.Money, Is.EqualTo(25));
        Assert.That(nextState.CurrentRound.Phase, Is.EqualTo(RoundPhase.PlayerTurn));
    }

    [Test]
    public void ContinueRoundEndAndShop_WhenBossBlindWasWon_StartsNextAnte() {
        RunState state = new RunState(
            currentRound: new RoundState(
                blind: new BlindState(BlindType.Boss, 1),
                targetScore: 600,
                currentScore: 600,
                handsLeft: 0,
                discardsLeft: 3,
                phase: RoundPhase.RoundEnd,
                maxHandSize: 1,
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
            currentShop: null,
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 40,
            phase: RunPhase.Blind
        );

        RunState shopState = RunReducer.Reduce(state, new ContinueRoundEndAction());
        RunState nextState = RunReducer.Reduce(shopState, new ContinueShopAction());

        Assert.That(nextState.CurrentBlind.Type, Is.EqualTo(BlindType.Small));
        Assert.That(nextState.CurrentRound.Ante, Is.EqualTo(2));
        Assert.That(nextState.CurrentRound.RoundNumber, Is.EqualTo(1));
        Assert.That(nextState.CurrentRound.TargetScore, Is.EqualTo(500));
        Assert.That(nextState.Money, Is.EqualTo(40));
    }

    [Test]
    public void ContinueRoundEnd_WhenRoundWasNotWon_ReturnsSameState() {
        RunState state = RunState.CreateInitial();

        RunState nextState = RunReducer.Reduce(state, new ContinueRoundEndAction());

        Assert.That(nextState, Is.SameAs(state));
    }

    [Test]
    public void EnterShop_WhenBlindWasWon_TransitionsRunToShopPhase() {
        RunState state = new RunState(
            currentRound: new RoundState(
                blind: new BlindState(BlindType.Small, 1),
                targetScore: 300,
                currentScore: 300,
                handsLeft: 0,
                discardsLeft: 3,
                phase: RoundPhase.RoundEnd,
                maxHandSize: 1,
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
            currentShop: null,
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Blind,
            runSeed: TestRunSeed
        );

        RunState nextState = RunReducer.Reduce(state, new ContinueRoundEndAction());

        Assert.That(nextState.Phase, Is.EqualTo(RunPhase.Shop));
        Assert.That(nextState.IsInShop, Is.True);
        Assert.That(nextState.CurrentShop, Is.Not.Null);
        Assert.That(nextState.CurrentShop.NextBlind.Type, Is.EqualTo(BlindType.Big));
        Assert.That(nextState.CurrentShop.Money, Is.EqualTo(25));
        Assert.That(nextState.CurrentShop.OfferPageIndex, Is.EqualTo(0));
        Assert.That(nextState.ShopRefreshCount, Is.EqualTo(1));
        Assert.That(nextState.CurrentShop.RunSeed, Is.EqualTo(TestRunSeed));
    }

    [Test]
    public void EnterShop_WhenRunHasPriorShopRefreshes_LoadsNextOfferPage() {
        RunState state = new RunState(
            currentRound: new RoundState(
                blind: new BlindState(BlindType.Big, 1),
                targetScore: 450,
                currentScore: 450,
                handsLeft: 0,
                discardsLeft: 3,
                phase: RoundPhase.RoundEnd,
                maxHandSize: 1,
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
            currentShop: null,
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Blind,
            shopRefreshCount: 1,
            runSeed: TestRunSeed
        );

        RunState nextState = RunReducer.Reduce(state, new ContinueRoundEndAction());
        var expectedOffers = JokerCatalog.CreateShopOffers(1, runSeed: TestRunSeed);

        Assert.That(nextState.CurrentShop.OfferPageIndex, Is.EqualTo(1));
        Assert.That(nextState.CurrentShop.FirstOffer.Id, Is.EqualTo(expectedOffers[0].Id));
        Assert.That(nextState.ShopRefreshCount, Is.EqualTo(2));
    }

    [Test]
    public void LeaveShop_WhenInShop_StartsPendingBlind() {
        RunState state = new RunState(
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
            currentShop: new ShopState(25, new BlindState(BlindType.Big, 1)),
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Shop
        );

        RunState nextState = RunReducer.Reduce(state, new ContinueShopAction());

        Assert.That(nextState.Phase, Is.EqualTo(RunPhase.Blind));
        Assert.That(nextState.CurrentShop, Is.Null);
        Assert.That(nextState.CurrentBlind.Type, Is.EqualTo(BlindType.Big));
        Assert.That(nextState.CurrentRound.Phase, Is.EqualTo(RoundPhase.PlayerTurn));
        Assert.That(nextState.Money, Is.EqualTo(25));
    }

    [Test]
    public void EnterShop_WhenRunAlreadyOwnsJoker_ExcludesOwnedJokerWhenPoolHasEnoughOptions() {
        RunState state = new RunState(
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
            currentShop: null,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) },
            money: 25,
            phase: RunPhase.Blind,
            runSeed: TestRunSeed
        );

        RunState nextState = RunReducer.Reduce(state, new ContinueRoundEndAction());

        Assert.That(nextState.CurrentShop, Is.Not.Null);
        for (int i = 0; i < nextState.CurrentShop.Offers.Count; i++) {
            Assert.That(nextState.CurrentShop.Offers[i].Id, Is.Not.EqualTo("glass-joker"));
            Assert.That(nextState.CurrentShop.Offers[i].IsPurchased, Is.False);
        }
    }

    [Test]
    public void BuySelectedShopOffer_WhenAffordable_AddsJokerToInventory() {
        RunState state = new RunState(
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
                25,
                new BlindState(BlindType.Big, 1),
                offers: CreateFixedShopOffers()),
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Shop
        );

        RunState nextState = RunReducer.Reduce(state, new BuyShopOfferAction(state.CurrentShop.SelectedOfferIndex));

        Assert.That(nextState.Money, Is.EqualTo(19));
        Assert.That(nextState.CurrentShop, Is.Not.Null);
        Assert.That(nextState.CurrentShop.FirstOffer, Is.Not.Null);
        Assert.That(nextState.CurrentShop.FirstOffer.IsPurchased, Is.True);
        Assert.That(nextState.OwnedJokers.Count, Is.EqualTo(1));
        Assert.That(nextState.OwnedJokers[0].Id, Is.EqualTo("glass-joker"));
    }

    [Test]
    public void BuySelectedShopOffer_WhenNotAffordable_KeepsStateUnchanged() {
        RunState state = new RunState(
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
                4,
                new BlindState(BlindType.Big, 1),
                new ShopOfferState[] {
                    new(JokerCatalog.GetById("glass-joker"))
                }),
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 4,
            phase: RunPhase.Shop
        );

        RunState nextState = RunReducer.Reduce(state, new BuyShopOfferAction(state.CurrentShop.SelectedOfferIndex));

        Assert.That(nextState, Is.SameAs(state));
    }

    [Test]
    public void SelectNextShopOffer_WhenInShop_ChangesSelectedOffer() {
        RunState state = new RunState(
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
                25,
                new BlindState(BlindType.Big, 1),
                offers: CreateFixedShopOffers()),
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Shop
        );

        RunState nextState = RunReducer.Reduce(state, new SelectShopOfferAction(1));

        Assert.That(nextState.CurrentShop.SelectedOfferIndex, Is.EqualTo(1));
        Assert.That(nextState.CurrentShop.SelectedOffer.Id, Is.EqualTo("ace-tag"));
    }

    [Test]
    public void SelectShopOffer_WhenIndexIsValid_ChangesSelectedOfferWithoutSpendingMoney() {
        RunState state = CreateShopRunState(money: 25);

        RunState nextState = RunReducer.Reduce(state, new SelectShopOfferAction(2));

        Assert.That(nextState.CurrentShop.SelectedOfferIndex, Is.EqualTo(2));
        Assert.That(nextState.CurrentShop.SelectedOffer.Id, Is.EqualTo("pair-glove"));
        Assert.That(nextState.Money, Is.EqualTo(25));
    }

    [Test]
    public void SelectShopOffer_WhenIndexIsInvalid_KeepsStateUnchanged() {
        RunState state = CreateShopRunState(money: 25);

        RunState nextState = RunReducer.Reduce(state, new SelectShopOfferAction(99));

        Assert.That(nextState, Is.SameAs(state));
    }

    [Test]
    public void BuyShopOffer_WhenIndexIsAffordable_BuysClickedOffer() {
        RunState state = CreateShopRunState(money: 25);

        RunState nextState = RunReducer.Reduce(state, new BuyShopOfferAction(1));

        Assert.That(nextState.Money, Is.EqualTo(17));
        Assert.That(nextState.CurrentShop.SelectedOfferIndex, Is.EqualTo(1));
        Assert.That(nextState.CurrentShop.SelectedOffer.IsPurchased, Is.True);
        Assert.That(nextState.OwnedJokers.Count, Is.EqualTo(1));
        Assert.That(nextState.OwnedJokers[0].Id, Is.EqualTo("ace-tag"));
    }

    [Test]
    public void BuyShopOffer_WhenIndexIsNotAffordable_KeepsStateUnchanged() {
        RunState state = CreateShopRunState(money: 4);

        RunState nextState = RunReducer.Reduce(state, new BuyShopOfferAction(0));

        Assert.That(nextState, Is.SameAs(state));
    }

    [Test]
    public void BuyShopOffer_WhenJokerIsAlreadyOwned_DoesNotDuplicateInventory() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) }
        );

        RunState nextState = RunReducer.Reduce(state, new BuyShopOfferAction(0));

        Assert.That(nextState.OwnedJokers.Count, Is.EqualTo(1));
        Assert.That(nextState.OwnedJokers[0].Id, Is.EqualTo("glass-joker"));
        Assert.That(nextState.CurrentShop.FirstOffer.IsPurchased, Is.True);
    }

    [Test]
    public void BuySelectedShopOffer_WhenAceTagWasSelected_BuysSelectedOffer() {
        RunState state = new RunState(
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
                25,
                new BlindState(BlindType.Big, 1),
                offers: CreateFixedShopOffers(),
                selectedOfferIndex: 1),
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Shop
        );

        RunState nextState = RunReducer.Reduce(state, new BuyShopOfferAction(state.CurrentShop.SelectedOfferIndex));

        Assert.That(nextState.Money, Is.EqualTo(17));
        Assert.That(nextState.OwnedJokers.Count, Is.EqualTo(1));
        Assert.That(nextState.OwnedJokers[0].Id, Is.EqualTo("ace-tag"));
        Assert.That(nextState.CurrentShop.SelectedOffer.IsPurchased, Is.True);
    }

    [Test]
    public void BuySelectedShopOffer_WhenJokerIsAlreadyOwned_DoesNotDuplicateInventory() {
        RunState state = new RunState(
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
                25,
                new BlindState(BlindType.Big, 1),
                new ShopOfferState[] {
                    new(JokerCatalog.GetById("glass-joker"))
                }),
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) },
            money: 25,
            phase: RunPhase.Shop
        );

        RunState nextState = RunReducer.Reduce(state, new BuyShopOfferAction(state.CurrentShop.SelectedOfferIndex));

        Assert.That(nextState.OwnedJokers.Count, Is.EqualTo(1));
        Assert.That(nextState.OwnedJokers[0].Id, Is.EqualTo("glass-joker"));
        Assert.That(nextState.CurrentShop.FirstOffer.IsPurchased, Is.True);
    }

    [Test]
    public void RerollShop_WhenInShop_SpendsMoneyAndChangesOfferPage() {
        RunState state = new RunState(
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
                25,
                new BlindState(BlindType.Big, 1),
                offers: CreateFixedShopOffers(),
                runSeed: TestRunSeed),
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Shop
        );

        RunState nextState = RunReducer.Reduce(state, new RerollShopAction());

        Assert.That(nextState.Money, Is.EqualTo(20));
        Assert.That(nextState.CurrentShop.RerollCount, Is.EqualTo(1));
        Assert.That(nextState.CurrentShop.RerollCost, Is.EqualTo(6));
        Assert.That(nextState.CurrentShop.OfferPageIndex, Is.EqualTo(1));
        Assert.That(nextState.CurrentShop.SelectedOfferIndex, Is.EqualTo(0));
        Assert.That(nextState.CurrentShop.Offers[0].Id, Is.Not.EqualTo(state.CurrentShop.Offers[0].Id));
        Assert.That(nextState.ShopRefreshCount, Is.EqualTo(1));
    }

    [Test]
    public void CreateShopOffers_WhenSeedAndPageMatch_ReturnsSameOffers() {
        var first = JokerCatalog.CreateShopOffers(2, runSeed: TestRunSeed);
        var second = JokerCatalog.CreateShopOffers(2, runSeed: TestRunSeed);

        Assert.That(GetOfferIds(second), Is.EqualTo(GetOfferIds(first)));
    }

    [Test]
    public void CreateShopOffers_WhenPageChanges_ReturnsDifferentOffers() {
        var first = JokerCatalog.CreateShopOffers(0, runSeed: TestRunSeed);
        var second = JokerCatalog.CreateShopOffers(1, runSeed: TestRunSeed);

        Assert.That(GetOfferIds(second), Is.Not.EqualTo(GetOfferIds(first)));
    }

    [Test]
    public void CreateShopOffers_WhenGenerated_DoesNotDuplicateJokersInSameShop() {
        var offers = JokerCatalog.CreateShopOffers(0, runSeed: TestRunSeed);

        Assert.That(offers, Has.Count.EqualTo(3));
        Assert.That(GetOfferIds(offers), Is.Unique);
    }

    [Test]
    public void CreateShopOffers_WhenMostJokersAreOwned_FillsWithPurchasedOwnedOffers() {
        JokerState[] ownedJokers = CreateOwnedJokers(
            "glass-joker",
            "ace-tag",
            "pair-glove",
            "club-chip",
            "straight-polish",
            "heart-tag",
            "flush-foil",
            "face-card-tag",
            "two-pair-grip",
            "spade-token",
            "cash-tag",
            "discard-pass",
            "triple-grip",
            "straight-engine",
            "pair-payout",
            "flush-mirror");

        var offers = JokerCatalog.CreateShopOffers(0, ownedJokers, TestRunSeed);

        Assert.That(offers, Has.Count.EqualTo(3));
        Assert.That(HasPurchasedOffer(offers), Is.True);
    }

    [Test]
    public void JokerCatalog_WhenRead_AllJokersHaveRarity() {
        foreach (JokerData joker in JokerCatalog.All) {
            Assert.That(System.Enum.IsDefined(typeof(JokerRarity), joker.Rarity), Is.True);
        }
    }

    [Test]
    public void JokerCatalog_WhenRead_HasMilestoneFourRarityPool() {
        int commonCount = 0;
        int uncommonCount = 0;
        int rareCount = 0;

        foreach (JokerData joker in JokerCatalog.All) {
            if (joker.Rarity == JokerRarity.Common) {
                commonCount++;
            } else if (joker.Rarity == JokerRarity.Uncommon) {
                uncommonCount++;
            } else if (joker.Rarity == JokerRarity.Rare) {
                rareCount++;
            }
        }

        Assert.That(JokerCatalog.All, Has.Count.EqualTo(18));
        Assert.That(commonCount, Is.EqualTo(8));
        Assert.That(uncommonCount, Is.EqualTo(6));
        Assert.That(rareCount, Is.EqualTo(4));
    }

    [Test]
    public void SellOwnedJoker_WhenNotInShop_KeepsStateUnchanged() {
        RunState state = new RunState(
            currentRound: RoundState.CreateInitial(),
            currentShop: null,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) },
            money: 10,
            phase: RunPhase.Blind
        );

        RunState nextState = RunReducer.Reduce(state, new SellOwnedJokerAction(0));

        Assert.That(nextState, Is.SameAs(state));
    }

    [Test]
    public void SellOwnedJoker_WhenIndexIsInvalid_KeepsStateUnchanged() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) }
        );

        RunState nextState = RunReducer.Reduce(state, new SellOwnedJokerAction(3));

        Assert.That(nextState, Is.SameAs(state));
    }

    [Test]
    public void SellOwnedJoker_WhenInShop_RemovesJokerAndAddsSellValue() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) }
        );

        RunState nextState = RunReducer.Reduce(state, new SellOwnedJokerAction(0));

        Assert.That(nextState.Money, Is.EqualTo(28));
        Assert.That(nextState.OwnedJokers, Has.Count.EqualTo(0));
    }

    [Test]
    public void SellOwnedJoker_WhenVisibleOfferMatchesSoldJoker_MakesOfferBuyableAgain() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) }
        );

        Assert.That(state.CurrentShop.FirstOffer.IsPurchased, Is.True);

        RunState nextState = RunReducer.Reduce(state, new SellOwnedJokerAction(0));

        Assert.That(nextState.CurrentShop.FirstOffer.Id, Is.EqualTo("glass-joker"));
        Assert.That(nextState.CurrentShop.FirstOffer.IsPurchased, Is.False);
        Assert.That(nextState.CurrentShop.FirstOffer.CanBuy(nextState.Money), Is.True);
    }

    [Test]
    public void PlaySelectedCards_WhenGlassJokerWasSold_DoesNotApplySoldJokerBonus() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.Ace, Suit.Hearts),
            TestCardFactory.Create(Rank.Three, Suit.Clubs),
            TestCardFactory.Create(Rank.Four, Suit.Diamonds),
            TestCardFactory.Create(Rank.Nine, Suit.Spades)
        };

        RunState state = CreateShopRunState(
            money: 10,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) }
        );

        state = RunReducer.Reduce(state, new SellOwnedJokerAction(0));
        state = RunReducer.Reduce(state, new ContinueShopAction(handCards));

        for (int i = 0; i < 5; i++) {
            state = RunReducer.Reduce(state, new ToggleCardSelectionAction(i));
        }

        RunState nextState = RunReducer.Reduce(state, new PlaySelectedCardsAction());

        Assert.That(nextState.CurrentRound.LastScoreResult.TotalChips, Is.EqualTo(32));
        Assert.That(nextState.CurrentRound.LastScoreResult.FinalScore, Is.EqualTo(64));
    }

    [Test]
    public void PlaySelectedCards_WhenGlassJokerWasBought_AddsBonusChipsToScore() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.Ace, Suit.Hearts),
            TestCardFactory.Create(Rank.Three, Suit.Clubs),
            TestCardFactory.Create(Rank.Four, Suit.Diamonds),
            TestCardFactory.Create(Rank.Nine, Suit.Spades)
        };

        RunState state = new RunState(
            currentRound: RoundState.CreateInitial(
                blind: new BlindState(BlindType.Small, 1),
                handsLeft: 2,
                maxHandSize: 5,
                initialHandCards: handCards
            ),
            currentShop: null,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) },
            money: 10,
            phase: RunPhase.Blind
        );

        for (int i = 0; i < 5; i++) {
            state = RunReducer.Reduce(state, new ToggleCardSelectionAction(i));
        }

        RunState nextState = RunReducer.Reduce(state, new PlaySelectedCardsAction());

        Assert.That(nextState.CurrentRound.LastScoreResult.TotalChips, Is.EqualTo(42));
        Assert.That(nextState.CurrentRound.LastScoreResult.FinalScore, Is.EqualTo(84));
    }

    [Test]
    public void PlaySelectedCards_WhenMoneyJokerMatches_AddsMoneyOnceAlongsideBlindReward() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.King, Suit.Spades),
            TestCardFactory.Create(Rank.Queen, Suit.Spades),
            TestCardFactory.Create(Rank.Jack, Suit.Spades),
            TestCardFactory.Create(Rank.Ten, Suit.Spades)
        };

        RunState state = new RunState(
            currentRound: RoundState.CreateInitial(
                blind: new BlindState(BlindType.Small, 1),
                handsLeft: 2,
                maxHandSize: 5,
                initialHandCards: handCards
            ),
            currentShop: null,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("cash-tag")) },
            money: 10,
            phase: RunPhase.Blind
        );

        for (int i = 0; i < 5; i++) {
            state = RunReducer.Reduce(state, new ToggleCardSelectionAction(i));
        }

        RunState scoringState = RunReducer.Reduce(state, new PlaySelectedCardsAction());

        Assert.That(scoringState.CurrentRound.Phase, Is.EqualTo(RoundPhase.Scoring));
        Assert.That(scoringState.CurrentRound.HasWonRound, Is.False);
        Assert.That(scoringState.Money, Is.EqualTo(12));
        StringAssert.Contains("Jokers: Cash Tag +$2", scoringState.CurrentRound.LastActionText);

        RunState nextState = RunReducer.Reduce(scoringState, new ScorePresentationFinishedAction());

        Assert.That(nextState.CurrentRound.HasWonRound, Is.True);
        Assert.That(nextState.Money, Is.EqualTo(22));
    }

    [Test]
    public void LeaveShop_WhenOwnsExtraResourceJokers_StartsPendingBlindWithBonuses() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: CreateOwnedJokers("spare-hand", "discard-pass")
        );

        RunState nextState = RunReducer.Reduce(state, new ContinueShopAction());

        Assert.That(nextState.CurrentRound.HandsLeft, Is.EqualTo(5));
        Assert.That(nextState.CurrentRound.DiscardsLeft, Is.EqualTo(4));
    }

    [Test]
    public void LeaveShop_WhenExtraHandJokerWasSold_RemovesFutureHandBonus() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: CreateOwnedJokers("spare-hand", "discard-pass")
        );

        RunState soldState = RunReducer.Reduce(state, new SellOwnedJokerAction(0));
        RunState nextState = RunReducer.Reduce(soldState, new ContinueShopAction());

        Assert.That(nextState.CurrentRound.HandsLeft, Is.EqualTo(4));
        Assert.That(nextState.CurrentRound.DiscardsLeft, Is.EqualTo(4));
    }

    [Test]
    public void BuyShopOffer_WhenInventoryIsFull_BlocksPurchaseUntilJokerIsSold() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: CreateOwnedJokers(
                "ace-tag",
                "pair-glove",
                "club-chip",
                "straight-polish",
                "heart-tag")
        );

        RunState blockedState = RunReducer.Reduce(state, new BuyShopOfferAction(0));

        Assert.That(blockedState, Is.SameAs(state));
        Assert.That(blockedState.OwnedJokers, Has.Count.EqualTo(RunState.MaxOwnedJokers));
        Assert.That(blockedState.CurrentShop.FirstOffer.IsPurchased, Is.False);

        RunState soldState = RunReducer.Reduce(blockedState, new SellOwnedJokerAction(0));
        RunState nextState = RunReducer.Reduce(soldState, new BuyShopOfferAction(0));

        Assert.That(nextState.OwnedJokers, Has.Count.EqualTo(RunState.MaxOwnedJokers));
        Assert.That(nextState.OwnedJokers[4].Id, Is.EqualTo("glass-joker"));
        Assert.That(nextState.CurrentShop.FirstOffer.IsPurchased, Is.True);
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
                offers: CreateFixedShopOffers(ownedJokers),
                runSeed: TestRunSeed),
            ownedJokers: ownedJokers ?? System.Array.Empty<JokerState>(),
            money: money,
            phase: RunPhase.Shop,
            runSeed: TestRunSeed
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

    private static string[] GetOfferIds(System.Collections.Generic.IReadOnlyList<ShopOfferState> offers) {
        var ids = new string[offers.Count];
        for (int i = 0; i < offers.Count; i++) {
            ids[i] = offers[i].Id;
        }

        return ids;
    }

    private static bool HasPurchasedOffer(System.Collections.Generic.IReadOnlyList<ShopOfferState> offers) {
        for (int i = 0; i < offers.Count; i++) {
            if (offers[i].IsPurchased) {
                return true;
            }
        }

        return false;
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
