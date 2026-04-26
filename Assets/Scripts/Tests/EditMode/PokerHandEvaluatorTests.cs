using NUnit.Framework;

public sealed class PokerHandEvaluatorTests {
    [Test]
    public void Evaluate_ReturnsStraightFlush_ForFiveCardStraightFlush() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Five, Suit.Clubs),
            TestCardFactory.Create(Rank.Six, Suit.Clubs),
            TestCardFactory.Create(Rank.Seven, Suit.Clubs),
            TestCardFactory.Create(Rank.Eight, Suit.Clubs),
            TestCardFactory.Create(Rank.Nine, Suit.Clubs)
        };

        PokerHandResult result = PokerHandEvaluator.Evaluate(cards);

        Assert.That(result.HandType, Is.EqualTo(PokerHandType.StraightFlush));
        Assert.That(result.IsAceLowStraight, Is.False);
    }

    [Test]
    public void Evaluate_ReturnsAceLowStraight_AndFlagsIt() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.Two, Suit.Hearts),
            TestCardFactory.Create(Rank.Three, Suit.Clubs),
            TestCardFactory.Create(Rank.Four, Suit.Diamonds),
            TestCardFactory.Create(Rank.Five, Suit.Spades)
        };

        PokerHandResult result = PokerHandEvaluator.Evaluate(cards);

        Assert.That(result.HandType, Is.EqualTo(PokerHandType.Straight));
        Assert.That(result.IsAceLowStraight, Is.True);
    }

    [Test]
    public void Evaluate_DoesNotTreatNonFiveCardSameSuitSelection_AsFlush() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Hearts),
            TestCardFactory.Create(Rank.Eight, Suit.Hearts),
            TestCardFactory.Create(Rank.Six, Suit.Hearts),
            TestCardFactory.Create(Rank.Three, Suit.Hearts)
        };

        PokerHandResult result = PokerHandEvaluator.Evaluate(cards);

        Assert.That(result.HandType, Is.EqualTo(PokerHandType.HighCard));
    }
}
