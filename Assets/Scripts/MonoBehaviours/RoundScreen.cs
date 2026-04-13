using Core;
using Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        var nextPhase = _roundState.Phase switch
        {
            RoundPhaseEnum.Waiting => RoundPhaseEnum.PlayerTurn,
            RoundPhaseEnum.PlayerTurn => RoundPhaseEnum.Scoring,
            RoundPhaseEnum.Scoring => RoundPhaseEnum.RoundEnd,
            _ => RoundPhaseEnum.Waiting
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
    }
}