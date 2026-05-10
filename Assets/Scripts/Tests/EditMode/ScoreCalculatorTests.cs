using Core;
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
    public void Calculate_UsesOnlyPairCards_ForPairHands() {
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
        Assert.That(result.CardChips, Is.EqualTo(22));
        Assert.That(result.TotalChips, Is.EqualTo(32));
        Assert.That(result.FinalScore, Is.EqualTo(64));
    }

    [Test]
    public void Calculate_WhenPairOfTwos_IgnoresKickerCard() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Two, Suit.Spades),
            TestCardFactory.Create(Rank.Two, Suit.Hearts),
            TestCardFactory.Create(Rank.Five, Suit.Clubs)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult);

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.Pair));
        Assert.That(result.CardChips, Is.EqualTo(4));
        Assert.That(result.TotalChips, Is.EqualTo(14));
        Assert.That(result.FinalScore, Is.EqualTo(28));
    }

    [Test]
    public void Calculate_WhenTwoPair_IgnoresKickerCard() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Ace, Suit.Spades),
            TestCardFactory.Create(Rank.Ace, Suit.Hearts),
            TestCardFactory.Create(Rank.Five, Suit.Clubs),
            TestCardFactory.Create(Rank.Five, Suit.Diamonds),
            TestCardFactory.Create(Rank.King, Suit.Spades)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult);

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.TwoPair));
        Assert.That(result.CardChips, Is.EqualTo(32));
        Assert.That(result.TotalChips, Is.EqualTo(52));
        Assert.That(result.FinalScore, Is.EqualTo(104));
    }

    [Test]
    public void Calculate_WhenThreeOfAKind_IgnoresKickerCards() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Queen, Suit.Spades),
            TestCardFactory.Create(Rank.Queen, Suit.Hearts),
            TestCardFactory.Create(Rank.Queen, Suit.Clubs),
            TestCardFactory.Create(Rank.Four, Suit.Diamonds),
            TestCardFactory.Create(Rank.Nine, Suit.Spades)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult);

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.ThreeOfAKind));
        Assert.That(result.CardChips, Is.EqualTo(30));
        Assert.That(result.TotalChips, Is.EqualTo(60));
        Assert.That(result.FinalScore, Is.EqualTo(180));
    }

    [Test]
    public void Calculate_WhenFourOfAKind_IgnoresKickerCard() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.King, Suit.Spades),
            TestCardFactory.Create(Rank.King, Suit.Hearts),
            TestCardFactory.Create(Rank.King, Suit.Clubs),
            TestCardFactory.Create(Rank.King, Suit.Diamonds),
            TestCardFactory.Create(Rank.Two, Suit.Spades)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult);

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.FourOfAKind));
        Assert.That(result.CardChips, Is.EqualTo(40));
        Assert.That(result.TotalChips, Is.EqualTo(100));
        Assert.That(result.FinalScore, Is.EqualTo(700));
    }

    [Test]
    public void Calculate_WhenFullHouse_UsesAllCards() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.King, Suit.Spades),
            TestCardFactory.Create(Rank.King, Suit.Hearts),
            TestCardFactory.Create(Rank.King, Suit.Clubs),
            TestCardFactory.Create(Rank.Five, Suit.Diamonds),
            TestCardFactory.Create(Rank.Five, Suit.Spades)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult);

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.FullHouse));
        Assert.That(result.CardChips, Is.EqualTo(40));
        Assert.That(result.TotalChips, Is.EqualTo(80));
        Assert.That(result.FinalScore, Is.EqualTo(320));
    }

    [Test]
    public void Calculate_WhenStraight_UsesAllCards() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Five, Suit.Spades),
            TestCardFactory.Create(Rank.Six, Suit.Hearts),
            TestCardFactory.Create(Rank.Seven, Suit.Clubs),
            TestCardFactory.Create(Rank.Eight, Suit.Diamonds),
            TestCardFactory.Create(Rank.Nine, Suit.Spades)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult);

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.Straight));
        Assert.That(result.CardChips, Is.EqualTo(35));
        Assert.That(result.TotalChips, Is.EqualTo(65));
        Assert.That(result.FinalScore, Is.EqualTo(260));
    }

    [Test]
    public void Calculate_WhenFlush_UsesAllCards() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Two, Suit.Hearts),
            TestCardFactory.Create(Rank.Five, Suit.Hearts),
            TestCardFactory.Create(Rank.Eight, Suit.Hearts),
            TestCardFactory.Create(Rank.Jack, Suit.Hearts),
            TestCardFactory.Create(Rank.King, Suit.Hearts)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult);

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.Flush));
        Assert.That(result.CardChips, Is.EqualTo(35));
        Assert.That(result.TotalChips, Is.EqualTo(70));
        Assert.That(result.FinalScore, Is.EqualTo(280));
    }

    [Test]
    public void Calculate_WhenStraightFlush_UsesAllCards() {
        CardData[] cards = {
            TestCardFactory.Create(Rank.Five, Suit.Clubs),
            TestCardFactory.Create(Rank.Six, Suit.Clubs),
            TestCardFactory.Create(Rank.Seven, Suit.Clubs),
            TestCardFactory.Create(Rank.Eight, Suit.Clubs),
            TestCardFactory.Create(Rank.Nine, Suit.Clubs)
        };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        ScoreResult result = ScoreCalculator.Calculate(cards, handResult);

        Assert.That(handResult.HandType, Is.EqualTo(PokerHandType.StraightFlush));
        Assert.That(result.CardChips, Is.EqualTo(35));
        Assert.That(result.TotalChips, Is.EqualTo(135));
        Assert.That(result.FinalScore, Is.EqualTo(1080));
    }

    [Test]
    public void GetScoringCardContributions_WhenHighCard_ReturnsHighestCardOnly() {
        CardData ace = TestCardFactory.Create(Rank.Ace, Suit.Spades);
        CardData seven = TestCardFactory.Create(Rank.Seven, Suit.Hearts);
        CardData three = TestCardFactory.Create(Rank.Three, Suit.Clubs);
        CardData[] cards = { ace, seven, three };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        var contributions = ScoreCalculator.GetScoringCardContributions(cards, handResult);

        Assert.That(contributions, Has.Count.EqualTo(1));
        Assert.That(contributions[0].Card, Is.EqualTo(ace));
        Assert.That(contributions[0].ChipValue, Is.EqualTo(11));
    }

    [Test]
    public void GetScoringCardContributions_WhenPair_ReturnsOnlyPairCards() {
        CardData aceSpades = TestCardFactory.Create(Rank.Ace, Suit.Spades);
        CardData aceHearts = TestCardFactory.Create(Rank.Ace, Suit.Hearts);
        CardData king = TestCardFactory.Create(Rank.King, Suit.Clubs);
        CardData[] cards = { aceSpades, aceHearts, king };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        var contributions = ScoreCalculator.GetScoringCardContributions(cards, handResult);

        Assert.That(contributions, Has.Count.EqualTo(2));
        Assert.That(contributions[0].Card, Is.EqualTo(aceSpades));
        Assert.That(contributions[0].ChipValue, Is.EqualTo(11));
        Assert.That(contributions[1].Card, Is.EqualTo(aceHearts));
        Assert.That(contributions[1].ChipValue, Is.EqualTo(11));
    }

    [Test]
    public void GetScoringCardContributions_WhenBossBlindDebuffsCard_SkipsDebuffedCard() {
        CardData aceClubs = TestCardFactory.Create(Rank.Ace, Suit.Clubs);
        CardData aceHearts = TestCardFactory.Create(Rank.Ace, Suit.Hearts);
        CardData king = TestCardFactory.Create(Rank.King, Suit.Spades);
        CardData[] cards = { aceClubs, aceHearts, king };

        PokerHandResult handResult = PokerHandEvaluator.Evaluate(cards);
        var contributions = ScoreCalculator.GetScoringCardContributions(cards, handResult, new BlindState(BlindType.Boss, 1));

        Assert.That(contributions, Has.Count.EqualTo(1));
        Assert.That(contributions[0].Card, Is.EqualTo(aceHearts));
        Assert.That(contributions[0].ChipValue, Is.EqualTo(11));
    }
}
