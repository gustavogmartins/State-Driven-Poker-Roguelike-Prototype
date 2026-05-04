using Core;
using NUnit.Framework;
using Presenters;
using View;

public sealed class RoundPresenterShopTests {
    [Test]
    public void Present_WhenRunIsInShop_CreatesThreeShopOfferViewModels() {
        RunState state = CreateShopRunState(money: 25);

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.ShopOffers, Has.Count.EqualTo(3));
        Assert.That(viewModel.ShopOffers[0].TitleText, Is.EqualTo("Glass Joker"));
        Assert.That(viewModel.ShopOffers[0].CostText, Is.EqualTo("$6"));
        Assert.That(viewModel.ShopOffers[0].StatusText, Is.EqualTo("Selected"));
        Assert.That(viewModel.ShopOffers[0].IsSelected, Is.True);
        Assert.That(viewModel.ShopOffers[0].CanBuy, Is.True);
        Assert.That(viewModel.ShopOffers[1].IsSelected, Is.False);
    }

    [Test]
    public void Present_WhenSelectedOfferIsBought_DisablesOfferAndShowsBoughtStatus() {
        RunState state = CreateShopRunState(
            money: 25,
            ownedJokers: new[] { new JokerState(JokerCatalog.GetById("glass-joker")) }
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.ShopOffers[0].IsPurchased, Is.True);
        Assert.That(viewModel.ShopOffers[0].CanBuy, Is.False);
        Assert.That(viewModel.ShopOffers[0].StatusText, Is.EqualTo("Bought"));
    }

    [Test]
    public void Present_WhenMoneyIsTooLow_ShowsNeedCostStatus() {
        RunState state = CreateShopRunState(money: 4);

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.ShopOffers[0].CanBuy, Is.False);
        Assert.That(viewModel.ShopOffers[0].StatusText, Is.EqualTo("Need $6"));
    }

    private static RunState CreateShopRunState(int money, JokerState[] ownedJokers = null) {
        RoundState wonRound = new RoundState(
            blind: new BlindState(BlindType.Small, 1),
            targetScore: 300,
            currentScore: 300,
            handsLeft: 1,
            discardsLeft: 3,
            phase: RoundPhase.RoundEnd,
            maxHandSize: 5,
            deckCards: System.Array.Empty<CardData>(),
            handCards: System.Array.Empty<CardData>(),
            discardPileCards: System.Array.Empty<CardData>(),
            selectedCardsIndexes: System.Array.Empty<int>(),
            lastActionText: "Blind cleared",
            lastPlayedCardsText: "None",
            lastPlayedCards: System.Array.Empty<CardData>(),
            lastPlayedCardsCount: 0,
            lastPlayedHandResult: PokerHandType.None,
            lastScoreResult: ScoreResult.Zero
        );

        return new RunState(
            currentRound: wonRound,
            currentShop: new ShopState(money, new BlindState(BlindType.Big, 1), ownedJokers: ownedJokers),
            ownedJokers: ownedJokers ?? System.Array.Empty<JokerState>(),
            money: money,
            phase: RunPhase.Shop
        );
    }
}
