using Core;
using Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using View;

public class RoundScreen : MonoBehaviour {
    [Header("Texts")] [SerializeField] private TextMeshProUGUI blindText;
    [SerializeField] private TextMeshProUGUI targetScore;
    [SerializeField] private TextMeshProUGUI currentScore;
    [SerializeField] private TextMeshProUGUI handsLeft;
    [SerializeField] private TextMeshProUGUI discardsLeft;
    [SerializeField] private TextMeshProUGUI phase;

    [Header("Buttons")] [SerializeField] private Button addScoreButton;
    [SerializeField] private Button useHandButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button nextPhaseButton;

    [SerializeField] private Transform handArea;
    [SerializeField] private CardView cardViewPrefab;

    private RoundPresenter _roundPresenter;
    private RoundState _roundState;

    private void Start() {
        _roundPresenter = new RoundPresenter();
        _roundState = RoundState.CreateDebug();

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

    public void OnDiscardButtonClicked() {
        var newDiscardsLeft = Mathf.Max(0, _roundState.DiscardsLeft - 1);
        _roundState = _roundState.WithHandsLeft(newDiscardsLeft);
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