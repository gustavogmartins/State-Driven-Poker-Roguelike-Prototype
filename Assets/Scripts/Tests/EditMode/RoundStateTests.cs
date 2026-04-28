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
    public void PlaySelectedCards_WhenTargetIsCleared_EndsRoundAndAwardsMoney() {
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
            money: 10,
            ante: 2,
            handsLeft: 2,
            maxHandSize: 5
        );

        RoundState nextState = state.PlaySelectedCards();

        Assert.That(nextState.IsRoundOver, Is.True);
        Assert.That(nextState.HasWonRound, Is.True);
        Assert.That(nextState.Phase, Is.EqualTo(RoundPhase.RoundEnd));
        Assert.That(nextState.HandsLeft, Is.EqualTo(1));
        Assert.That(nextState.Money, Is.EqualTo(30));
        Assert.That(nextState.LastPlayedHandResult, Is.EqualTo(PokerHandType.StraightFlush));
        StringAssert.Contains("Blind cleared", nextState.LastActionText);
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

        RoundState nextState = state.PlaySelectedCards();

        Assert.That(nextState.IsRoundOver, Is.True);
        Assert.That(nextState.HasLostRound, Is.True);
        Assert.That(nextState.Phase, Is.EqualTo(RoundPhase.RoundEnd));
        Assert.That(nextState.Money, Is.EqualTo(state.Money));
        StringAssert.Contains("Round lost", nextState.LastActionText);
    }

    [Test]
    public void DiscardCards_ConsumesDiscard_AndRefillsTheHand() {
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

        RoundState nextState = state.DiscardCards();

        Assert.That(nextState.DiscardsLeft, Is.EqualTo(0));
        Assert.That(nextState.HandCards, Has.Count.EqualTo(3));
        Assert.That(nextState.DeckCards, Has.Count.EqualTo(0));
        Assert.That(nextState.DiscardPileCards, Has.Count.EqualTo(2));
        Assert.That(nextState.SelectedCardsCount, Is.EqualTo(0));
        StringAssert.Contains("No discards left", nextState.LastActionText);
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
    public void StartNextBlind_WhenRoundWasWon_AdvancesBlindAndKeepsMoney() {
        var state = CreateState(
            handCards: new[] { TestCardFactory.Create(Rank.Ace, Suit.Spades) },
            blind: new BlindState(BlindType.Small, 1),
            targetScore: 300,
            currentScore: 300,
            money: 25,
            handsLeft: 0,
            phase: RoundPhase.RoundEnd,
            maxHandSize: 1
        );

        RoundState nextState = state.StartNextBlind();

        Assert.That(nextState.Blind.Type, Is.EqualTo(BlindType.Big));
        Assert.That(nextState.Ante, Is.EqualTo(1));
        Assert.That(nextState.RoundNumber, Is.EqualTo(2));
        Assert.That(nextState.TargetScore, Is.EqualTo(450));
        Assert.That(nextState.Money, Is.EqualTo(25));
        Assert.That(nextState.Phase, Is.EqualTo(RoundPhase.PlayerTurn));
        Assert.That(nextState.CurrentScore, Is.EqualTo(0));
        Assert.That(nextState.HandsLeft, Is.EqualTo(4));
        Assert.That(nextState.DiscardsLeft, Is.EqualTo(3));
    }

    [Test]
    public void StartNextBlind_WhenBossBlindWasWon_StartsNextAnte() {
        var state = CreateState(
            handCards: new[] { TestCardFactory.Create(Rank.Ace, Suit.Spades) },
            blind: new BlindState(BlindType.Boss, 1),
            targetScore: 600,
            currentScore: 600,
            money: 40,
            handsLeft: 0,
            phase: RoundPhase.RoundEnd,
            maxHandSize: 1
        );

        RoundState nextState = state.StartNextBlind();

        Assert.That(nextState.Blind.Type, Is.EqualTo(BlindType.Small));
        Assert.That(nextState.Ante, Is.EqualTo(2));
        Assert.That(nextState.RoundNumber, Is.EqualTo(1));
        Assert.That(nextState.TargetScore, Is.EqualTo(500));
        Assert.That(nextState.Money, Is.EqualTo(40));
    }

    [Test]
    public void StartNextBlind_WhenRoundWasNotWon_ReturnsSameState() {
        var state = CreateState(
            handCards: new[] { TestCardFactory.Create(Rank.Ace, Suit.Spades) },
            blind: new BlindState(BlindType.Small, 1),
            currentScore: 100,
            targetScore: 300,
            phase: RoundPhase.PlayerTurn,
            maxHandSize: 1
        );

        RoundState nextState = state.StartNextBlind();

        Assert.That(nextState, Is.SameAs(state));
    }

    private static RoundState CreateState(
        IReadOnlyList<CardData> handCards,
        BlindState blind = null,
        IReadOnlyList<CardData> deckCards = null,
        IReadOnlyList<int> selectedIndexes = null,
        int? targetScore = null,
        int currentScore = 0,
        int money = 10,
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
            money: money,
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
