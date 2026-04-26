using NUnit.Framework;

public sealed class ScoreCalculatorTests {
    [Test]
    public void Calculate_UsesOnlyHighestCard_ForHighCardHands() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.Seven, Suit.Hearts),
            TestCardFactory.Create(Rank.Three, Suit.Clubs)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult);

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.HighCard));
        Assert.That(result.BaseChips, Is.EqualTo(5));
        Assert.That(result.BaseMult, Is.EqualTo(1));
        Assert.That(result.CardChips, Is.EqualTo(11));
        Assert.That(result.TotalChips, Is.EqualTo(16));
        Assert.That(result.FinalScore, Is.EqualTo(16));
    }

    [Test]
    public void Calculate_UsesAllPlayedCards_ForPairHands() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.Ace, Suit.Hearts),
            TestCardFactory.Create(Rank.Three, Suit.Clubs),
            TestCardFactory.Create(Rank.Four, Suit.Diamonds),
            TestCardFactory.Create(Rank.Nine, Suit.Spades)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult);

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.Pair));
        Assert.That(result.BaseChips, Is.EqualTo(10));
        Assert.That(result.BaseMult, Is.EqualTo(2));
        Assert.That(result.CardChips, Is.EqualTo(38));
        Assert.That(result.TotalChips, Is.EqualTo(48));
        Assert.That(result.FinalScore, Is.EqualTo(96));
    }
}
