using System.Collections.Generic;
using Core;
using NUnit.Framework;

public sealed class RoundStateTests {
    [Test]
    public void Constructor_SanitizesSelectedIndexes() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.King, Suit.Hearts),
            TestCardFactory.Create(Rank.Queen, Suit.Clubs)
        };

        var state = CreateState(
            handCards: handCards,
            selectedIndexes: new[] { 2, 2, -1, 7, 1, 0 }
        );

        Assert.That(state.SelectedCardsIndexes, Is.EqualTo(new[] { 0, 1, 2 }));
    }

    [Test]
    public void PlaySelectedCards_WhenTargetIsCleared_EndsRound() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Five, Suit.Clubs),
            TestCardFactory.Create(Rank.Six, Suit.Clubs),
            TestCardFactory.Create(Rank.Seven, Suit.Clubs),
            TestCardFactory.Create(Rank.Eight, Suit.Clubs),
            TestCardFactory.Create(Rank.Nine, Suit.Clubs)
        };

        var state = CreateState(
            handCards: handCards,
            selectedIndexes: new[] { 0, 1, 2, 3, 4 },
            targetScore: 100,
            ante: 2,
            handsLeft: 2,
            maxHandSize: 5
        );

        RoundState scoringState = RoundReducer.Reduce(state, new PlaySelectedCardsAction());

        Assert.That(scoringState.IsRoundOver, Is.False);
        Assert.That(scoringState.HasWonRound, Is.False);
        Assert.That(scoringState.Phase, Is.EqualTo(RoundPhase.Scoring));
        Assert.That(scoringState.HandsLeft, Is.EqualTo(1));
        Assert.That(scoringState.PlayedCards, Has.Count.EqualTo(5));
        Assert.That(scoringState.HandCards, Has.Count.EqualTo(0));
        Assert.That(scoringState.DiscardPileCards, Has.Count.EqualTo(0));
        Assert.That(scoringState.LastPlayedHandResult, Is.EqualTo(PokerHandType.StraightFlush));
        StringAssert.Contains("Blind cleared", scoringState.LastActionText);

        RoundState nextState = RoundReducer.Reduce(scoringState, new ScorePresentationFinishedAction());

        Assert.That(nextState.IsRoundOver, Is.True);
        Assert.That(nextState.HasWonRound, Is.True);
        Assert.That(nextState.Phase, Is.EqualTo(RoundPhase.RoundEnd));
        Assert.That(nextState.PlayedCards, Is.Empty);
        Assert.That(nextState.DiscardPileCards, Has.Count.EqualTo(5));
    }

    [Test]
    public void PlaySelectedCards_WhenLastHandFails_EndsRoundAsLoss() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Two, Suit.Spades)
        };

        var state = CreateState(
            handCards: handCards,
            selectedIndexes: new[] { 0 },
            targetScore: 100,
            handsLeft: 1,
            maxHandSize: 1
        );

        RoundState scoringState = RoundReducer.Reduce(state, new PlaySelectedCardsAction());

        Assert.That(scoringState.IsRoundOver, Is.False);
        Assert.That(scoringState.HasLostRound, Is.False);
        Assert.That(scoringState.Phase, Is.EqualTo(RoundPhase.Scoring));
        StringAssert.Contains("Round lost", scoringState.LastActionText);

        RoundState nextState = RoundReducer.Reduce(scoringState, new ScorePresentationFinishedAction());

        Assert.That(nextState.IsRoundOver, Is.True);
        Assert.That(nextState.HasLostRound, Is.True);
        Assert.That(nextState.Phase, Is.EqualTo(RoundPhase.RoundEnd));
    }

    [Test]
    public void ScorePresentationFinished_WhenRoundContinues_RefillsHandAndReturnsToPlayerTurn() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.King, Suit.Hearts),
            TestCardFactory.Create(Rank.Queen, Suit.Clubs)
        };

        CardData[] deckCards = {
            TestCardFactory.Create(Rank.Two, Suit.Diamonds),
            TestCardFactory.Create(Rank.Three, Suit.Spades)
        };

        var state = CreateState(
            handCards: handCards,
            deckCards: deckCards,
            selectedIndexes: new[] { 0, 1 },
            targetScore: 1000,
            handsLeft: 2,
            maxHandSize: 3
        );

        RoundState scoringState = RoundReducer.Reduce(state, new PlaySelectedCardsAction());

        Assert.That(scoringState.Phase, Is.EqualTo(RoundPhase.Scoring));
        Assert.That(scoringState.HandCards, Has.Count.EqualTo(1));
        Assert.That(scoringState.PlayedCards, Has.Count.EqualTo(2));
        Assert.That(scoringState.DeckCards, Has.Count.EqualTo(2));
        Assert.That(scoringState.DiscardPileCards, Is.Empty);

        RoundState nextState = RoundReducer.Reduce(scoringState, new ScorePresentationFinishedAction());

        Assert.That(nextState.Phase, Is.EqualTo(RoundPhase.PlayerTurn));
        Assert.That(nextState.HandCards, Has.Count.EqualTo(3));
        Assert.That(nextState.PlayedCards, Is.Empty);
        Assert.That(nextState.DeckCards, Is.Empty);
        Assert.That(nextState.DiscardPileCards, Has.Count.EqualTo(2));
        Assert.That(nextState.SelectedCardsCount, Is.EqualTo(0));
    }

    [Test]
    public void ScoringPhase_DisablesPlayerActions() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.King, Suit.Hearts)
        };

        var state = CreateState(
            handCards: handCards,
            selectedIndexes: new[] { 0 },
            targetScore: 1000,
            handsLeft: 2,
            maxHandSize: 2
        );

        RoundState scoringState = RoundReducer.Reduce(state, new PlaySelectedCardsAction());

        Assert.That(scoringState.CanPlaySelectedCards, Is.False);
        Assert.That(scoringState.CanDiscardSelectedCards, Is.False);
        Assert.That(scoringState.CanSortHand, Is.False);
        Assert.That(RoundReducer.Reduce(scoringState, new ToggleCardSelectionAction(0)), Is.SameAs(scoringState));
    }

    [Test]
    public void DiscardCards_ConsumesDiscardAndWaitsForPresentation() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.King, Suit.Hearts),
            TestCardFactory.Create(Rank.Queen, Suit.Clubs)
        };

        CardData[] deckCards = {
            TestCardFactory.Create(Rank.Two, Suit.Diamonds),
            TestCardFactory.Create(Rank.Three, Suit.Spades)
        };

        var state = CreateState(
            handCards: handCards,
            deckCards: deckCards,
            selectedIndexes: new[] { 0, 1 },
            discardsLeft: 1,
            maxHandSize: 3
        );

        RoundState discardingState = RoundReducer.Reduce(state, new DiscardSelectedCardsAction());

        Assert.That(discardingState.Phase, Is.EqualTo(RoundPhase.Discarding));
        Assert.That(discardingState.DiscardsLeft, Is.EqualTo(0));
        Assert.That(discardingState.HandCards, Has.Count.EqualTo(1));
        Assert.That(discardingState.DiscardedCards, Has.Count.EqualTo(2));
        Assert.That(discardingState.DeckCards, Has.Count.EqualTo(2));
        Assert.That(discardingState.DiscardPileCards, Is.Empty);
        Assert.That(discardingState.SelectedCardsCount, Is.EqualTo(0));
        StringAssert.Contains("No discards left", discardingState.LastActionText);
    }

    [Test]
    public void DiscardPresentationFinished_RefillsHandAndReturnsToPlayerTurn() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.King, Suit.Hearts),
            TestCardFactory.Create(Rank.Queen, Suit.Clubs)
        };

        CardData[] deckCards = {
            TestCardFactory.Create(Rank.Two, Suit.Diamonds),
            TestCardFactory.Create(Rank.Three, Suit.Spades)
        };

        var state = CreateState(
            handCards: handCards,
            deckCards: deckCards,
            selectedIndexes: new[] { 0, 1 },
            discardsLeft: 1,
            maxHandSize: 3
        );

        RoundState discardingState = RoundReducer.Reduce(state, new DiscardSelectedCardsAction());
        RoundState nextState = RoundReducer.Reduce(discardingState, new DiscardPresentationFinishedAction());

        Assert.That(nextState.Phase, Is.EqualTo(RoundPhase.PlayerTurn));
        Assert.That(nextState.DiscardsLeft, Is.EqualTo(0));
        Assert.That(nextState.HandCards, Has.Count.EqualTo(3));
        Assert.That(nextState.DiscardedCards, Is.Empty);
        Assert.That(nextState.DeckCards, Is.Empty);
        Assert.That(nextState.DiscardPileCards, Has.Count.EqualTo(2));
    }

    [Test]
    public void DiscardingPhase_DisablesPlayerActions() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.King, Suit.Hearts)
        };

        var state = CreateState(
            handCards: handCards,
            selectedIndexes: new[] { 0 },
            discardsLeft: 1,
            maxHandSize: 2
        );

        RoundState discardingState = RoundReducer.Reduce(state, new DiscardSelectedCardsAction());

        Assert.That(discardingState.CanPlaySelectedCards, Is.False);
        Assert.That(discardingState.CanDiscardSelectedCards, Is.False);
        Assert.That(discardingState.CanSortHand, Is.False);
        Assert.That(RoundReducer.Reduce(discardingState, new ToggleCardSelectionAction(0)), Is.SameAs(discardingState));
    }

    [Test]
    public void Constructor_UsesBlindStateForDerivedBlindValues() {
        var state = CreateState(
            handCards: new[] { TestCardFactory.Create(Rank.Ace, Suit.Spades) },
            blind: new BlindState(BlindType.Big, 2),
            maxHandSize: 1
        );

        Assert.That(state.BlindName, Is.EqualTo("Big Blind"));
        Assert.That(state.Ante, Is.EqualTo(2));
        Assert.That(state.RoundNumber, Is.EqualTo(2));
        Assert.That(state.BlindReward, Is.EqualTo(30));
        Assert.That(state.TargetScore, Is.EqualTo(750));
    }

    [Test]
    public void CreateInitial_StartsWithoutPersistentHandSort() {
        RoundState state = RoundState.CreateInitial();

        Assert.That(state.HandSortMode, Is.EqualTo(HandSortMode.None));
    }

    [Test]
    public void ScorePresentationFinished_WhenHandWasSortedByRank_RefillsAndKeepsRankSort() {
        CardData twoClubs = TestCardFactory.Create(Rank.Two, Suit.Clubs);
        CardData kingHearts = TestCardFactory.Create(Rank.King, Suit.Hearts);
        CardData aceSpades = TestCardFactory.Create(Rank.Ace, Suit.Spades);
        CardData queenDiamonds = TestCardFactory.Create(Rank.Queen, Suit.Diamonds);

        var state = CreateState(
            handCards: new[] { twoClubs, kingHearts, aceSpades },
            deckCards: new[] { queenDiamonds },
            targetScore: 1000,
            handsLeft: 2,
            maxHandSize: 3
        );

        RoundState sortedState = RoundReducer.Reduce(state, new SortHandByRankAction());
        RoundState selectedState = RoundReducer.Reduce(sortedState, new ToggleCardSelectionAction(0));
        RoundState scoringState = RoundReducer.Reduce(selectedState, new PlaySelectedCardsAction());
        RoundState nextState = RoundReducer.Reduce(scoringState, new ScorePresentationFinishedAction());

        Assert.That(nextState.HandSortMode, Is.EqualTo(HandSortMode.Rank));
        Assert.That(nextState.HandCards, Is.EqualTo(new[] { kingHearts, queenDiamonds, twoClubs }));
        Assert.That(nextState.SelectedCardsCount, Is.EqualTo(0));
    }

    [Test]
    public void DiscardPresentationFinished_WhenHandWasSortedBySuit_RefillsAndKeepsSuitSort() {
        CardData aceHearts = TestCardFactory.Create(Rank.Ace, Suit.Hearts);
        CardData kingClubs = TestCardFactory.Create(Rank.King, Suit.Clubs);
        CardData queenSpades = TestCardFactory.Create(Rank.Queen, Suit.Spades);
        CardData aceClubs = TestCardFactory.Create(Rank.Ace, Suit.Clubs);

        var state = CreateState(
            handCards: new[] { aceHearts, kingClubs, queenSpades },
            deckCards: new[] { aceClubs },
            discardsLeft: 1,
            maxHandSize: 3
        );

        RoundState sortedState = RoundReducer.Reduce(state, new SortHandBySuitAction());
        RoundState selectedState = RoundReducer.Reduce(sortedState, new ToggleCardSelectionAction(1));
        RoundState discardingState = RoundReducer.Reduce(selectedState, new DiscardSelectedCardsAction());
        RoundState nextState = RoundReducer.Reduce(discardingState, new DiscardPresentationFinishedAction());

        Assert.That(nextState.HandSortMode, Is.EqualTo(HandSortMode.Suit));
        Assert.That(nextState.HandCards, Is.EqualTo(new[] { aceClubs, kingClubs, queenSpades }));
        Assert.That(nextState.SelectedCardsCount, Is.EqualTo(0));
    }

    private static RoundState CreateState(
        IReadOnlyList<CardData> handCards,
        BlindState blind = null,
        IReadOnlyList<CardData> deckCards = null,
        IReadOnlyList<int> selectedIndexes = null,
        int? targetScore = null,
        int currentScore = 0,
        int ante = 1,
        int handsLeft = 4,
        int discardsLeft = 3,
        RoundPhase phase = RoundPhase.PlayerTurn,
        int maxHandSize = 8) {
        BlindState resolvedBlind = blind ?? new BlindState(BlindType.Small, ante);

        return new RoundState(
            blind: resolvedBlind,
            targetScore: targetScore ?? resolvedBlind.TargetScore,
            currentScore: currentScore,
            handsLeft: handsLeft,
            discardsLeft: discardsLeft,
            phase: phase,
            maxHandSize: maxHandSize,
            deckCards: deckCards ?? new List<CardData>(),
            handCards: handCards,
            discardPileCards: new List<CardData>(),
            selectedCardsIndexes: selectedIndexes ?? new List<int>(),
            lastActionText: "Waiting for input",
            lastPlayedCardsText: "None",
            lastPlayedCards: new List<CardData>(),
            lastPlayedCardsCount: 0,
            lastPlayedHandResult: PokerHandType.None,
            lastScoreResult: ScoreResult.Zero
        );
    }
}
