using Core;
using NUnit.Framework;
using Presenters;
using View;

public sealed class RoundPresenterAnimationTests {
    [Test]
    public void Present_HandCards_ExposeCardIdAndHandZone() {
        CardData[] handCards = {
            new CardData(101, Rank.Ace, Suit.Spades),
            new CardData(102, Rank.King, Suit.Hearts)
        };

        RunState state = RunState.CreateInitial(
            maxHandSize: 2,
            initialHandCards: handCards
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.HandCards[0].CardId, Is.EqualTo(101));
        Assert.That(viewModel.HandCards[0].Zone, Is.EqualTo(CardZone.Hand));
        Assert.That(viewModel.HandCards[1].CardId, Is.EqualTo(102));
        Assert.That(viewModel.HandCards[1].Zone, Is.EqualTo(CardZone.Hand));
    }

    [Test]
    public void Present_PlayedCards_ExposeCardIdAndPlayedZone() {
        CardData playedCard = new CardData(201, Rank.Queen, Suit.Clubs);

        var roundState = new RoundState(
            blind: new BlindState(BlindType.Small, 1),
            targetScore: 300,
            currentScore: 20,
            handsLeft: 3,
            discardsLeft: 3,
            phase: RoundPhase.PlayerTurn,
            maxHandSize: 5,
            deckCards: System.Array.Empty<CardData>(),
            handCards: System.Array.Empty<CardData>(),
            discardPileCards: new[] { playedCard },
            selectedCardsIndexes: System.Array.Empty<int>(),
            lastActionText: "Played High Card for 20",
            lastPlayedCardsText: "Q\u2663",
            lastPlayedCards: new[] { playedCard },
            lastPlayedCardsCount: 1,
            lastPlayedHandResult: PokerHandType.HighCard,
            lastScoreResult: new ScoreResult(5, 1, 15, 20, 20)
        );

        var state = new RunState(
            currentRound: roundState,
            currentShop: null,
            ownedJokers: System.Array.Empty<JokerState>(),
            money: 10,
            phase: RunPhase.Blind
        );

        RoundViewModel viewModel = new RoundPresenter().Present(state);

        Assert.That(viewModel.PlayedCards[0].CardId, Is.EqualTo(201));
        Assert.That(viewModel.PlayedCards[0].Zone, Is.EqualTo(CardZone.Played));
    }
}
