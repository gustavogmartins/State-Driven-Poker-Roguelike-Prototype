using Core;
using NUnit.Framework;
using Presenters;
using View;

public sealed class RoundPresenterAnimationTests {
    [Test]
    public void Present_HandCards_ExposeCardIdAndHandZone() {
        CardData[] handCards = {
            new CardData(101, Rank.Ace, Suit.Spades),
            new CardData(102, Rank.King, Suit.Hearts)
        };

        RunState state = RunState.CreateInitial(
            maxHandSize: 2,
            initialHandCards: handCards
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.HandCards[0].CardId, Is.EqualTo(101));
        Assert.That(viewModel.HandCards[0].Zone, Is.EqualTo(CardZone.Hand));
        Assert.That(viewModel.HandCards[1].CardId, Is.EqualTo(102));
        Assert.That(viewModel.HandCards[1].Zone, Is.EqualTo(CardZone.Hand));
    }

    [Test]
    public void Present_WhenPairIsSelected_ShowsBaseChipsInsteadOfCardChipTotal() {
        CardData[] handCards = {
            new CardData(111, Rank.Ace, Suit.Spades),
            new CardData(112, Rank.Ace, Suit.Hearts),
            new CardData(113, Rank.King, Suit.Spades)
        };

        RunState state = RunState.CreateInitial(
            maxHandSize: 3,
            initialHandCards: handCards
        );

        state = RunReducer.Reduce(state, new ToggleCardSelectionAction(0));
        state = RunReducer.Reduce(state, new ToggleCardSelectionAction(1));
        state = RunReducer.Reduce(state, new ToggleCardSelectionAction(2));

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.ChipsText, Is.EqualTo("10"));
        Assert.That(viewModel.MultText, Is.EqualTo("2"));
        Assert.That(viewModel.ScoreBaseChips, Is.EqualTo(10));
        Assert.That(viewModel.ScoreTargetChips, Is.EqualTo(32));
    }

    [Test]
    public void Present_PlayedCards_ExposeCardIdAndPlayedZone() {
        CardData playedCard = new CardData(201, Rank.Queen, Suit.Clubs);

        var roundState = new RoundState(
            blind: new BlindState(BlindType.Small, 1),
            targetScore: 300,
            currentScore: 20,
            handsLeft: 3,
            discardsLeft: 3,
            phase: RoundPhase.PlayerTurn,
            maxHandSize: 5,
            deckCards: System.Array.Empty<CardData>(),
            handCards: System.Array.Empty<CardData>(),
            discardPileCards: new[] { playedCard },
            selectedCardsIndexes: System.Array.Empty<int>(),
            lastActionText: "Played High Card for 20",
            lastPlayedCardsText: "Q\u2663",
            lastPlayedCards: new[] { playedCard },
            lastPlayedCardsCount: 1,
            lastPlayedHandResult: PokerHandType.HighCard,
            lastScoreResult: new ScoreResult(5, 1, 15, 20, 20),
            playedCards: new[] { playedCard }
        );

        var state = new RunState(
            currentRound: roundState,
            currentShop: null,
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 10,
            phase: RunPhase.Blind
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.PlayedCards[0].CardId, Is.EqualTo(201));
        Assert.That(viewModel.PlayedCards[0].Zone, Is.EqualTo(CardZone.Played));
    }

    [Test]
    public void Present_WhenScoring_DisablesInputAndExposesPlayedCards() {
        CardData[] handCards = {
            new CardData(301, Rank.Ace, Suit.Spades),
            new CardData(302, Rank.King, Suit.Hearts)
        };

        RunState state = RunState.CreateInitial(
            maxHandSize: 2,
            initialHandCards: handCards
        );

        state = RunReducer.Reduce(state, new ToggleCardSelectionAction(0));
        state = RunReducer.Reduce(state, new PlaySelectedCardsAction());

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.Phase, Is.EqualTo(RoundPhase.Scoring));
        Assert.That(viewModel.CanPlayHand, Is.False);
        Assert.That(viewModel.CanDiscard, Is.False);
        Assert.That(viewModel.CanSort, Is.False);
        Assert.That(viewModel.HandCards[0].IsInteractable, Is.False);
        Assert.That(viewModel.PlayedCards[0].CardId, Is.EqualTo(301));
        Assert.That(viewModel.PlayedCards[0].Zone, Is.EqualTo(CardZone.Played));
    }

    [Test]
    public void Present_WhenScoring_ShowsBaseScorePresentationValuesAndPreviousRoundScore() {
        CardData aceSpades = new CardData(501, Rank.Ace, Suit.Spades);
        CardData aceHearts = new CardData(502, Rank.Ace, Suit.Hearts);
        CardData kingSpades = new CardData(503, Rank.King, Suit.Spades);
        var baseScore = new ScoreResult(10, 2, 22, 32, 64);

        var roundState = new RoundState(
            blind: new BlindState(BlindType.Small, 1),
            targetScore: 300,
            currentScore: 164,
            handsLeft: 3,
            discardsLeft: 3,
            phase: RoundPhase.Scoring,
            maxHandSize: 5,
            deckCards: System.Array.Empty<CardData>(),
            handCards: System.Array.Empty<CardData>(),
            discardPileCards: System.Array.Empty<CardData>(),
            selectedCardsIndexes: System.Array.Empty<int>(),
            lastActionText: "Played Pair for 64",
            lastPlayedCardsText: "A\u2660, A\u2665, K\u2660",
            lastPlayedCards: new[] { aceSpades, aceHearts, kingSpades },
            lastPlayedCardsCount: 3,
            lastPlayedHandResult: PokerHandType.Pair,
            lastScoreResult: baseScore,
            playedCards: new[] { aceSpades, aceHearts, kingSpades },
            lastBaseScoreResult: baseScore
        );

        var state = new RunState(
            currentRound: roundState,
            currentShop: null,
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 10,
            phase: RunPhase.Blind
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.RoundScoreText, Is.EqualTo("100"));
        Assert.That(viewModel.ChipsText, Is.EqualTo("10"));
        Assert.That(viewModel.MultText, Is.EqualTo("2"));
        Assert.That(viewModel.HasScorePresentation, Is.True);
        Assert.That(viewModel.ScoreStartRoundScore, Is.EqualTo(100));
        Assert.That(viewModel.ScoreTargetRoundScore, Is.EqualTo(164));
        Assert.That(viewModel.ScoreBaseChips, Is.EqualTo(10));
        Assert.That(viewModel.ScoreTargetChips, Is.EqualTo(32));
        Assert.That(viewModel.ScoreBaseMult, Is.EqualTo(2));
        Assert.That(viewModel.ScoreTargetBaseMult, Is.EqualTo(2));
        Assert.That(viewModel.ScoreTargetMultMultiplier, Is.EqualTo(1));
        Assert.That(viewModel.ScoreBonusChips, Is.EqualTo(0));
        Assert.That(viewModel.PlayedCards[0].IsScoringCard, Is.True);
        Assert.That(viewModel.PlayedCards[0].ScoringChipValue, Is.EqualTo(11));
        Assert.That(viewModel.PlayedCards[1].IsScoringCard, Is.True);
        Assert.That(viewModel.PlayedCards[1].ScoringChipValue, Is.EqualTo(11));
        Assert.That(viewModel.PlayedCards[2].IsScoringCard, Is.False);
        Assert.That(viewModel.PlayedCards[2].ScoringChipValue, Is.EqualTo(0));
    }

    [Test]
    public void Present_WhenScoringWithJokerModifiers_PreservesBaseDisplayAndExposesAggregateDelta() {
        CardData[] handCards = {
            new CardData(601, Rank.Ace, Suit.Spades),
            new CardData(602, Rank.Ace, Suit.Hearts),
            new CardData(603, Rank.King, Suit.Spades)
        };

        RunState state = new RunState(
            currentRound: RoundState.CreateInitial(
                blind: new BlindState(BlindType.Small, 1),
                maxHandSize: 3,
                initialHandCards: handCards
            ),
            currentShop: null,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) },
            money: 10,
            phase: RunPhase.Blind
        );

        state = RunReducer.Reduce(state, new ToggleCardSelectionAction(0));
        state = RunReducer.Reduce(state, new ToggleCardSelectionAction(1));
        state = RunReducer.Reduce(state, new ToggleCardSelectionAction(2));
        state = RunReducer.Reduce(state, new PlaySelectedCardsAction());

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(state.CurrentRound.CurrentScore, Is.EqualTo(84));
        Assert.That(state.CurrentRound.LastBaseScoreResult.TotalChips, Is.EqualTo(32));
        Assert.That(state.CurrentRound.LastScoreResult.TotalChips, Is.EqualTo(42));
        Assert.That(viewModel.RoundScoreText, Is.EqualTo("0"));
        Assert.That(viewModel.ChipsText, Is.EqualTo("10"));
        Assert.That(viewModel.MultText, Is.EqualTo("2"));
        Assert.That(viewModel.ScoreTargetChips, Is.EqualTo(42));
        Assert.That(viewModel.ScoreBonusChips, Is.EqualTo(10));
        Assert.That(viewModel.ScoreFinalScore, Is.EqualTo(84));
    }

    [Test]
    public void Present_WhenDiscarding_DisablesInputAndExposesDiscardedCards() {
        CardData[] handCards = {
            new CardData(401, Rank.Ace, Suit.Spades),
            new CardData(402, Rank.King, Suit.Hearts)
        };

        RunState state = RunState.CreateInitial(
            maxHandSize: 2,
            initialHandCards: handCards
        );

        state = RunReducer.Reduce(state, new ToggleCardSelectionAction(0));
        state = RunReducer.Reduce(state, new DiscardSelectedCardsAction());

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.Phase, Is.EqualTo(RoundPhase.Discarding));
        Assert.That(viewModel.CanPlayHand, Is.False);
        Assert.That(viewModel.CanDiscard, Is.False);
        Assert.That(viewModel.CanSort, Is.False);
        Assert.That(viewModel.HandCards[0].IsInteractable, Is.False);
        Assert.That(viewModel.DiscardedCards[0].CardId, Is.EqualTo(401));
        Assert.That(viewModel.DiscardedCards[0].Zone, Is.EqualTo(CardZone.Discard));
    }
}
