using NUnit.Framework;

public sealed class CardDataTests {
    [Test]
    public void Constructor_AssignsUniqueAutomaticInstanceIds() {
        var first = new CardData(Rank.Ace, Suit.Spades);
        var second = new CardData(Rank.Ace, Suit.Spades);

        Assert.That(first.InstanceId, Is.GreaterThan(0));
        Assert.That(second.InstanceId, Is.GreaterThan(0));
        Assert.That(second.InstanceId, Is.Not.EqualTo(first.InstanceId));
    }

    [Test]
    public void Constructor_WithExplicitInstanceId_PreservesValue() {
        var card = new CardData(1234, Rank.King, Suit.Hearts);

        Assert.That(card.InstanceId, Is.EqualTo(1234));
        Assert.That(card.Rank, Is.EqualTo(Rank.King));
        Assert.That(card.Suit, Is.EqualTo(Suit.Hearts));
    }
}
