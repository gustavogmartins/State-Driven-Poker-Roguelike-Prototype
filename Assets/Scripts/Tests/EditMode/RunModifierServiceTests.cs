using Core;
using NUnit.Framework;

public sealed class RunModifierServiceTests {
    [Test]
    public void ApplyScoreModifiers_WhenHandContainsAce_AddsAceTagMult() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.Ace, Suit.Hearts),
            TestCardFactory.Create(Rank.Four, Suit.Clubs)
        };

        ScoreResult result = Apply(cards, "ace-tag");

        Assert.That(result.BaseMult, Is.EqualTo(6));
        Assert.That(result.FinalScore, Is.EqualTo(result.TotalChips * 6));
    }

    [Test]
    public void ApplyScoreModifiers_WhenHandIsPair_AddsPairGloveChips() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.King, Suit.Spades),
            TestCardFactory.Create(Rank.King, Suit.Hearts),
            TestCardFactory.Create(Rank.Three, Suit.Clubs)
        };

        ScoreResult baseScore = CalculateBase(cards);
        ScoreResult result = Apply(cards, "pair-glove");

        Assert.That(result.TotalChips, Is.EqualTo(baseScore.TotalChips + 20));
        Assert.That(result.BaseMult, Is.EqualTo(baseScore.BaseMult));
        Assert.That(result.FinalScore, Is.EqualTo((baseScore.TotalChips + 20) * baseScore.BaseMult));
    }

    [Test]
    public void ApplyScoreModifiers_WhenHandIsFlush_AddsFlushFoilChips() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Two, Suit.Hearts),
            TestCardFactory.Create(Rank.Five, Suit.Hearts),
            TestCardFactory.Create(Rank.Eight, Suit.Hearts),
            TestCardFactory.Create(Rank.Jack, Suit.Hearts),
            TestCardFactory.Create(Rank.King, Suit.Hearts)
        };

        ScoreResult baseScore = CalculateBase(cards);
        ScoreResult result = Apply(cards, "flush-foil");

        Assert.That(result.TotalChips, Is.EqualTo(baseScore.TotalChips + 25));
        Assert.That(result.FinalScore, Is.EqualTo((baseScore.TotalChips + 25) * baseScore.BaseMult));
    }

    [Test]
    public void ApplyScoreModifiers_WhenHandContainsClub_AddsClubChipChips() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.Seven, Suit.Clubs),
            TestCardFactory.Create(Rank.Three, Suit.Diamonds)
        };

        ScoreResult baseScore = CalculateBase(cards);
        ScoreResult result = Apply(cards, "club-chip");

        Assert.That(result.TotalChips, Is.EqualTo(baseScore.TotalChips + 15));
        Assert.That(result.FinalScore, Is.EqualTo((baseScore.TotalChips + 15) * baseScore.BaseMult));
    }

    [Test]
    public void ApplyScoreModifiers_WhenHandContainsFaceCard_AddsFaceCardTagMult() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.King, Suit.Spades),
            TestCardFactory.Create(Rank.Seven, Suit.Clubs),
            TestCardFactory.Create(Rank.Three, Suit.Diamonds)
        };

        ScoreResult baseScore = CalculateBase(cards);
        ScoreResult result = Apply(cards, "face-card-tag");

        Assert.That(result.BaseMult, Is.EqualTo(baseScore.BaseMult + 4));
        Assert.That(result.TotalChips, Is.EqualTo(baseScore.TotalChips));
        Assert.That(result.FinalScore, Is.EqualTo(baseScore.TotalChips * (baseScore.BaseMult + 4)));
    }

    [Test]
    public void ApplyScoreModifiers_WhenMultipleJokersMatch_ComposesChipsAndMult() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Clubs),
            TestCardFactory.Create(Rank.Ace, Suit.Hearts),
            TestCardFactory.Create(Rank.King, Suit.Spades)
        };

        ScoreResult baseScore = CalculateBase(cards);
        ScoreResult result = Apply(cards, "glass-joker", "ace-tag", "club-chip", "face-card-tag");

        int expectedChips = baseScore.TotalChips + 10 + 15;
        int expectedMult = baseScore.BaseMult + 4 + 4;

        Assert.That(result.TotalChips, Is.EqualTo(expectedChips));
        Assert.That(result.BaseMult, Is.EqualTo(expectedMult));
        Assert.That(result.FinalScore, Is.EqualTo(expectedChips * expectedMult));
    }

    private static ScoreResult Apply(CardData[] cards, params string[] jokerIds) {
        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult baseScore = ScoreCalculator.Calculate(cards, handResult);
        var jokers = new JokerState[jokerIds.Length];

        for (int i = 0; i < jokerIds.Length; i++) {
            jokers[i] = new JokerState(JokerCatalog.GetById(jokerIds[i]));
        }

        return RunModifierService.ApplyScoreModifiers(baseScore, jokers, cards, handResult);
    }

    private static ScoreResult CalculateBase(CardData[] cards) {
        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        return ScoreCalculator.Calculate(cards, handResult);
    }
}
