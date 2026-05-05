using Core;
using NUnit.Framework;

public sealed class BossBlindScoringTests {
    [Test]
    public void Calculate_WhenBossBlindAndScoringCardIsClub_RemovesClubCardChips() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Clubs),
            TestCardFactory.Create(Rank.Seven, Suit.Hearts),
            TestCardFactory.Create(Rank.Three, Suit.Diamonds)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult, new BlindState(BlindType.Boss, 1));

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.HighCard));
        Assert.That(result.CardChips, Is.EqualTo(0));
        Assert.That(result.TotalChips, Is.EqualTo(5));
        Assert.That(result.FinalScore, Is.EqualTo(5));
    }

    [Test]
    public void Calculate_WhenSmallBlindAndScoringCardIsClub_KeepsClubCardChips() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Clubs),
            TestCardFactory.Create(Rank.Seven, Suit.Hearts),
            TestCardFactory.Create(Rank.Three, Suit.Diamonds)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult, new BlindState(BlindType.Small, 1));

        Assert.That(result.CardChips, Is.EqualTo(11));
        Assert.That(result.TotalChips, Is.EqualTo(16));
        Assert.That(result.FinalScore, Is.EqualTo(16));
    }

    [Test]
    public void Calculate_WhenBossBlindAndPairContainsClub_RemovesOnlyClubCardChips() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Clubs),
            TestCardFactory.Create(Rank.Ace, Suit.Hearts),
            TestCardFactory.Create(Rank.King, Suit.Spades)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult, new BlindState(BlindType.Boss, 1));

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.Pair));
        Assert.That(result.CardChips, Is.EqualTo(11));
        Assert.That(result.TotalChips, Is.EqualTo(21));
        Assert.That(result.FinalScore, Is.EqualTo(42));
    }

    [Test]
    public void Calculate_WhenBossBlindAndAllPairCardsAreDebuffed_KeepsOnlyBaseHandChips() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Clubs),
            TestCardFactory.Create(Rank.Ace, Suit.Clubs),
            TestCardFactory.Create(Rank.King, Suit.Spades)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult, new BlindState(BlindType.Boss, 1));

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.Pair));
        Assert.That(result.BaseChips, Is.EqualTo(10));
        Assert.That(result.CardChips, Is.EqualTo(0));
        Assert.That(result.TotalChips, Is.EqualTo(10));
        Assert.That(result.FinalScore, Is.EqualTo(20));
    }

    [Test]
    public void Calculate_WhenBossBlindAndStraightContainsClubs_RemovesOnlyDebuffedCardChips() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Five, Suit.Clubs),
            TestCardFactory.Create(Rank.Six, Suit.Hearts),
            TestCardFactory.Create(Rank.Seven, Suit.Clubs),
            TestCardFactory.Create(Rank.Eight, Suit.Diamonds),
            TestCardFactory.Create(Rank.Nine, Suit.Spades)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult, new BlindState(BlindType.Boss, 1));

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.Straight));
        Assert.That(result.CardChips, Is.EqualTo(23));
        Assert.That(result.TotalChips, Is.EqualTo(53));
        Assert.That(result.FinalScore, Is.EqualTo(212));
    }

    [Test]
    public void Calculate_WhenBossBlindAndClubFlush_KeepsOnlyBaseHandChips() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Two, Suit.Clubs),
            TestCardFactory.Create(Rank.Five, Suit.Clubs),
            TestCardFactory.Create(Rank.Eight, Suit.Clubs),
            TestCardFactory.Create(Rank.Jack, Suit.Clubs),
            TestCardFactory.Create(Rank.King, Suit.Clubs)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult, new BlindState(BlindType.Boss, 1));

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.Flush));
        Assert.That(result.BaseChips, Is.EqualTo(35));
        Assert.That(result.CardChips, Is.EqualTo(0));
        Assert.That(result.TotalChips, Is.EqualTo(35));
        Assert.That(result.FinalScore, Is.EqualTo(140));
    }
}
