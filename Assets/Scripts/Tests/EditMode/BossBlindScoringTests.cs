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
        Assert.That(result.CardChips, Is.EqualTo(21));
        Assert.That(result.TotalChips, Is.EqualTo(31));
        Assert.That(result.FinalScore, Is.EqualTo(62));
    }
}
