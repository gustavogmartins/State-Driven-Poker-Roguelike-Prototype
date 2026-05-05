using Core;
using NUnit.Framework;
using Presenters;
using View;

public sealed class RoundPresenterBossTests {
    [Test]
    public void Present_WhenBossBlindHasClubCards_MarksClubCardsAsDebuffed() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Ace, Suit.Clubs),
            TestCardFactory.Create(Rank.King, Suit.Spades)
        };

        RunState state = RunState.CreateInitial(
            blind: new BlindState(BlindType.Boss, 1),
            maxHandSize: 2,
            initialHandCards: handCards
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.HandCards[0].IsDebuffed, Is.True);
        Assert.That(viewModel.HandCards[1].IsDebuffed, Is.False);
    }

    [Test]
    public void Present_WhenSelectedHandHasXMultJoker_ShowsMultiplierSuffix() {
        CardData[] handCards = {
            TestCardFactory.Create(Rank.Five, Suit.Spades),
            TestCardFactory.Create(Rank.Six, Suit.Hearts),
            TestCardFactory.Create(Rank.Seven, Suit.Clubs),
            TestCardFactory.Create(Rank.Eight, Suit.Diamonds),
            TestCardFactory.Create(Rank.Nine, Suit.Spades)
        };

        RunState state = new RunState(
            currentRound: RoundState.CreateInitial(
                blind: new BlindState(BlindType.Small, 1),
                maxHandSize: 5,
                initialHandCards: handCards
            ),
            currentShop: null,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("straight-engine")) },
            money: 10,
            phase: RunPhase.Blind
        );

        for (int i = 0; i < 5; i++) {
            state = state.ToggleCardSelection(i);
        }

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        StringAssert.Contains(" x2", viewModel.MultText);
    }
}
