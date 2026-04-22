using System.Collections.Generic;
using Core;
using Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using View;

public class RoundScreen : MonoBehaviour {
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI blindText;

    [SerializeField] private TextMeshProUGUI targetScore;
    [SerializeField] private TextMeshProUGUI currentScore;
    [SerializeField] private TextMeshProUGUI handsLeft;
    [SerializeField] private TextMeshProUGUI discardsLeft;
    [SerializeField] private TextMeshProUGUI phase;
    [SerializeField] private TextMeshProUGUI lastPlayedCardsText;
    [SerializeField] private TextMeshProUGUI lastActionText;
    [SerializeField] private TextMeshProUGUI lastPlayedCountText;
    [SerializeField] private TextMeshProUGUI discardPileCountText;
    [SerializeField] private TextMeshProUGUI topDiscardText;
    [SerializeField] private TextMeshProUGUI playedHandTypeText;

    [Header("Debug Buttons")]
    [SerializeField] private Button addScoreButtonDebug;

    [SerializeField] private Button useHandButtonDebug;
    [SerializeField] private Button discardButtonDebug;
    [SerializeField] private Button nextPhaseButtonDebug;

    [Header("HandButtons")]
    [SerializeField] private Button playHandButton;

    [SerializeField] private Button discardButton;
    [SerializeField] private Button sortByRankButton;
    [SerializeField] private Button sortBySuitButton;

    [Header("Hand Area")]
    [SerializeField] private Transform handArea;

    [SerializeField] private TextMeshProUGUI handSizeText;

    [Header("Played Hand Area")]
    [SerializeField] private Transform playedHandArea;

    [Header("Card Prefab")]
    [SerializeField] private CardView cardViewPrefab;

    [Header("Deck Area")]
    [SerializeField] private TextMeshProUGUI deckCountText;

    [Header("Debug")]
    [SerializeField] private bool useDebugHandScenario = false;

    [SerializeField] private DebugHandScenario debugHandScenario = DebugHandScenario.None;

    private RoundPresenter _roundPresenter;
    private RoundState _roundState;

    private void Start() {
        _roundPresenter = new RoundPresenter();
        _roundState = CreateInitialState();

        Render(_roundState);
    }

    private RoundState CreateInitialState() {
        var fullDeck = DeckBuilder.CreateStandard52();
        var shuffledDeck = DeckShuffler.Shuffle(fullDeck);

        if (!useDebugHandScenario || debugHandScenario == DebugHandScenario.None) {
            var normalDraw = DeckUtility.DrawCards(shuffledDeck, 8);

            return new RoundState(
                blindName: "Small Blind",
                targetScore: 300,
                currentScore: 0,
                handsLeft: 4,
                discardsLeft: 3,
                phase: RoundPhase.Waiting,
                maxHandSize: 8,
                deckCards: normalDraw.RemainingDeck,
                handCards: normalDraw.DrawnCards,
                discardPileCards: new List<CardData>(),
                selectedCardsIndexes: new List<int>(),
                lastActionText: "None",
                lastPlayedCardsText: "None",
                lastPlayedCardsCount: 0,
                lastPlayedHandType: PokerHandType.None
            );
        }

        var debugHand = DebugHandFactory.Create(debugHandScenario);

        var remainingDeck = RemoveCardsFromDeck(shuffledDeck, debugHand);

        return new RoundState(
            blindName: "Small Blind",
            targetScore: 300,
            currentScore: 0,
            handsLeft: 4,
            discardsLeft: 3,
            phase: RoundPhase.Waiting,
            maxHandSize: 8,
            deckCards: remainingDeck,
            handCards: debugHand,
            discardPileCards: new List<CardData>(),
            selectedCardsIndexes: new List<int>(),
            lastActionText: "None",
            lastPlayedCardsText: "None",
            lastPlayedCardsCount: 0,
            lastPlayedHandType: PokerHandType.None
        );
    }

    public void OnPlayHandButtonClicked() {
        _roundState = _roundState.PlaySelectedCards();
        Render(_roundState);
    }

    public void OnDiscardButtonClicked() {
        _roundState = _roundState.DiscardCards();
        Render(_roundState);
    }

    public void OnAddScoreButtonClicked() {
        _roundState = _roundState.WithScore(_roundState.CurrentScore + 100);
        Render(_roundState);
    }

    public void OnUseHandButtonClicked() {
        var newHandsLeft = Mathf.Max(0, _roundState.HandsLeft - 1);
        _roundState = _roundState.WithHandsLeft(newHandsLeft);
        Render(_roundState);
    }

    public void OnNextPhaseButtonClicked() {
        var nextPhase = _roundState.Phase switch {
            RoundPhase.Waiting => RoundPhase.PlayerTurn,
            RoundPhase.PlayerTurn => RoundPhase.Scoring,
            RoundPhase.Scoring => RoundPhase.RoundEnd,
            _ => RoundPhase.Waiting
        };

        _roundState = _roundState.WithPhase(nextPhase);
        Render(_roundState);
    }

    private void Render(RoundState roundState) {
        var viewModel = _roundPresenter.Present(roundState);

        blindText.text = viewModel.BlindText;
        targetScore.text = viewModel.TargetScoreText;
        currentScore.text = viewModel.CurrentScoreText;
        handsLeft.text = viewModel.HandsLeftText;
        discardsLeft.text = viewModel.DiscardsLeftText;
        phase.text = viewModel.PhaseText;
        lastPlayedCardsText.text = viewModel.LastPlayedCardsText;
        lastActionText.text = viewModel.LastActionText;
        lastPlayedCountText.text = viewModel.LastPlayedCountText;
        handSizeText.text = viewModel.HandSizeText;
        deckCountText.text = viewModel.DeckCountText;
        discardPileCountText.text = viewModel.DiscardPileCountText;
        topDiscardText.text = viewModel.TopDiscardText;
        playedHandTypeText.text = viewModel.PlayedHandTypeText;
        RenderHand(viewModel);
    }

    private void RenderHand(RoundViewModel viewModel) {
        ClearHand();

        foreach (var cardVm in viewModel.HandCards) {
            var cardView = Instantiate(cardViewPrefab, handArea);
            cardView.Bind(cardVm);
            cardView.OnCardSelected += OnCardSelected;
        }
    }

    private List<CardData> RemoveCardsFromDeck(List<CardData> deck, List<CardData> cardsToRemove) {
        var remainingDeck = new List<CardData>(deck);

        foreach (var cardToRemove in cardsToRemove) {
            for (int i = 0; i < remainingDeck.Count; i++) {
                if (remainingDeck[i].Rank == cardToRemove.Rank &&
                    remainingDeck[i].Suit == cardToRemove.Suit) {
                    remainingDeck.RemoveAt(i);
                    break;
                }
            }
        }

        return remainingDeck;
    }

    private void OnCardSelected(int index) {
        _roundState = _roundState.ToggleCardSelection(index);
        Render(_roundState);
    }

    private void ClearHand() {
        for (int i = handArea.childCount - 1; i >= 0; i--) {
            Destroy(handArea.GetChild(i).gameObject);
        }
    }
}