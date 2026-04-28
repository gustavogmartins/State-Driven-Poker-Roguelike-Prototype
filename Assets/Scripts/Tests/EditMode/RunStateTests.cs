using Core;
using NUnit.Framework;

public sealed class RunStateTests {
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
            state = state.ToggleCardSelection(i);
        }

        RunState nextState = state.PlaySelectedCards();

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

        state = state.ToggleCardSelection(0);

        RunState nextState = state.PlaySelectedCards();

        Assert.That(nextState.CurrentRound.HasLostRound, Is.True);
        Assert.That(nextState.IsRunOver, Is.True);
        Assert.That(nextState.Phase, Is.EqualTo(RunPhase.RunEnd));
    }

    [Test]
    public void StartNextBlind_WhenRoundWasWon_AdvancesBlindAndKeepsMoney() {
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

        RunState nextState = state.StartNextBlind();

        Assert.That(nextState.CurrentBlind.Type, Is.EqualTo(BlindType.Big));
        Assert.That(nextState.CurrentRound.Ante, Is.EqualTo(1));
        Assert.That(nextState.CurrentRound.RoundNumber, Is.EqualTo(2));
        Assert.That(nextState.CurrentRound.TargetScore, Is.EqualTo(450));
        Assert.That(nextState.Money, Is.EqualTo(25));
        Assert.That(nextState.CurrentRound.Phase, Is.EqualTo(RoundPhase.PlayerTurn));
    }

    [Test]
    public void StartNextBlind_WhenBossBlindWasWon_StartsNextAnte() {
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

        RunState nextState = state.StartNextBlind();

        Assert.That(nextState.CurrentBlind.Type, Is.EqualTo(BlindType.Small));
        Assert.That(nextState.CurrentRound.Ante, Is.EqualTo(2));
        Assert.That(nextState.CurrentRound.RoundNumber, Is.EqualTo(1));
        Assert.That(nextState.CurrentRound.TargetScore, Is.EqualTo(500));
        Assert.That(nextState.Money, Is.EqualTo(40));
    }

    [Test]
    public void StartNextBlind_WhenRoundWasNotWon_ReturnsSameState() {
        RunState state = RunState.CreateInitial();

        RunState nextState = state.StartNextBlind();

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
            phase: RunPhase.Blind
        );

        RunState nextState = state.EnterShop();

        Assert.That(nextState.Phase, Is.EqualTo(RunPhase.Shop));
        Assert.That(nextState.IsInShop, Is.True);
        Assert.That(nextState.CurrentShop, Is.Not.Null);
        Assert.That(nextState.CurrentShop.NextBlind.Type, Is.EqualTo(BlindType.Big));
        Assert.That(nextState.CurrentShop.Money, Is.EqualTo(25));
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

        RunState nextState = state.LeaveShop();

        Assert.That(nextState.Phase, Is.EqualTo(RunPhase.Blind));
        Assert.That(nextState.CurrentShop, Is.Null);
        Assert.That(nextState.CurrentBlind.Type, Is.EqualTo(BlindType.Big));
        Assert.That(nextState.CurrentRound.Phase, Is.EqualTo(RoundPhase.PlayerTurn));
        Assert.That(nextState.Money, Is.EqualTo(25));
    }

    [Test]
    public void EnterShop_WhenRunAlreadyOwnsJoker_MarksOfferAsPurchased() {
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
            phase: RunPhase.Blind
        );

        RunState nextState = state.EnterShop();

        Assert.That(nextState.CurrentShop, Is.Not.Null);
        Assert.That(nextState.CurrentShop.FirstOffer, Is.Not.Null);
        Assert.That(nextState.CurrentShop.FirstOffer.Id, Is.EqualTo("glass-joker"));
        Assert.That(nextState.CurrentShop.FirstOffer.IsPurchased, Is.True);
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
            currentShop: new ShopState(25, new BlindState(BlindType.Big, 1)),
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Shop
        );

        RunState nextState = state.BuySelectedShopOffer();

        Assert.That(nextState.Money, Is.EqualTo(19));
        Assert.That(nextState.CurrentShop, Is.Not.Null);
        Assert.That(nextState.CurrentShop.FirstOffer, Is.Not.Null);
        Assert.That(nextState.CurrentShop.FirstOffer.IsPurchased, Is.True);
        Assert.That(nextState.OwnedJokers.Count, Is.EqualTo(1));
        Assert.That(nextState.OwnedJokers[0].Id, Is.EqualTo("glass-joker"));
        Assert.That(nextState.CanBuySelectedShopOffer, Is.False);
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

        RunState nextState = state.BuySelectedShopOffer();

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
            currentShop: new ShopState(25, new BlindState(BlindType.Big, 1)),
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Shop
        );

        RunState nextState = state.SelectNextShopOffer();

        Assert.That(nextState.CurrentShop.SelectedOfferIndex, Is.EqualTo(1));
        Assert.That(nextState.CurrentShop.SelectedOffer.Id, Is.EqualTo("ace-tag"));
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
            currentShop: new ShopState(25, new BlindState(BlindType.Big, 1), selectedOfferIndex: 1),
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Shop
        );

        RunState nextState = state.BuySelectedShopOffer();

        Assert.That(nextState.Money, Is.EqualTo(17));
        Assert.That(nextState.OwnedJokers.Count, Is.EqualTo(1));
        Assert.That(nextState.OwnedJokers[0].Id, Is.EqualTo("ace-tag"));
        Assert.That(nextState.CurrentShop.SelectedOffer.IsPurchased, Is.True);
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
            currentShop: new ShopState(25, new BlindState(BlindType.Big, 1)),
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 25,
            phase: RunPhase.Shop
        );

        RunState nextState = state.RerollShop();

        Assert.That(nextState.Money, Is.EqualTo(24));
        Assert.That(nextState.CurrentShop.RerollCount, Is.EqualTo(1));
        Assert.That(nextState.CurrentShop.SelectedOfferIndex, Is.EqualTo(0));
        Assert.That(nextState.CurrentShop.Offers[0].Id, Is.EqualTo("club-chip"));
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
            state = state.ToggleCardSelection(i);
        }

        RunState nextState = state.PlaySelectedCards();

        Assert.That(nextState.CurrentRound.LastScoreResult.TotalChips, Is.EqualTo(58));
        Assert.That(nextState.CurrentRound.LastScoreResult.FinalScore, Is.EqualTo(116));
    }
}
