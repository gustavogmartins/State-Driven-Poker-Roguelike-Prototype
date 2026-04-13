using System;
using Core;
using Presenters;
using TMPro;
using UnityEngine;

public class RoundScreen : MonoBehaviour {
    [Header("Texts")] 
    [SerializeField] private TextMeshProUGUI blindText;
    [SerializeField] private TextMeshProUGUI targetScore;
    [SerializeField] private TextMeshProUGUI currentScore;
    [SerializeField] private TextMeshProUGUI handsLeft;
    [SerializeField] private TextMeshProUGUI discardsLeft;
    [SerializeField] private TextMeshProUGUI phase;

    private RoundPresenter _roundPresenter;
    private RoundState _roundState;

    private void Start() {
        _roundPresenter = new RoundPresenter();
        _roundState = RoundState.CreateDebug();

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
