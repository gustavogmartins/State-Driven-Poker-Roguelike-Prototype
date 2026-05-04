using Core;
using NUnit.Framework;

public sealed class BlindStateTests {
    [Test]
    public void CreateFirst_StartsAtSmallBlindOnAnteOne() {
        BlindState blind = BlindState.CreateFirst();

        Assert.That(blind.Type, Is.EqualTo(BlindType.Small));
        Assert.That(blind.Ante, Is.EqualTo(1));
        Assert.That(blind.RoundNumber, Is.EqualTo(1));
        Assert.That(blind.Name, Is.EqualTo("Small Blind"));
        Assert.That(blind.Reward, Is.EqualTo(10));
        Assert.That(blind.TargetScore, Is.EqualTo(300));
    }

    [Test]
    public void Advance_FromSmallBlind_MovesToBigBlindInSameAnte() {
        var blind = new BlindState(BlindType.Small, 2);

        BlindState nextBlind = blind.Advance();

        Assert.That(nextBlind.Type, Is.EqualTo(BlindType.Big));
        Assert.That(nextBlind.Ante, Is.EqualTo(2));
        Assert.That(nextBlind.RoundNumber, Is.EqualTo(2));
        Assert.That(nextBlind.Name, Is.EqualTo("Big Blind"));
        Assert.That(nextBlind.Reward, Is.EqualTo(30));
        Assert.That(nextBlind.TargetScore, Is.EqualTo(750));
    }

    [Test]
    public void Advance_FromBossBlind_StartsNextAnteAtSmallBlind() {
        var blind = new BlindState(BlindType.Boss, 3);

        BlindState nextBlind = blind.Advance();

        Assert.That(nextBlind.Type, Is.EqualTo(BlindType.Small));
        Assert.That(nextBlind.Ante, Is.EqualTo(4));
        Assert.That(nextBlind.RoundNumber, Is.EqualTo(1));
        Assert.That(nextBlind.Name, Is.EqualTo("Small Blind"));
        Assert.That(nextBlind.Reward, Is.EqualTo(40));
        Assert.That(nextBlind.TargetScore, Is.EqualTo(900));
    }
}
