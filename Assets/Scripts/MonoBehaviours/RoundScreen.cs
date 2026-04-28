using System.Collections.Generic;
using Core;
using Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using View;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoundScreen : MonoBehaviour {
    private static readonly Color32 OverlayWinColor = new(244, 158, 27, 255);
    private static readonly Color32 OverlayLossColor = new(214, 72, 72, 255);

    [Header("Left Panel")]
    [SerializeField] private TextMeshProUGUI blindTitleText;
    [SerializeField] private TextMeshProUGUI blindDescriptionText;
    [SerializeField] private TextMeshProUGUI blindRequirementText;
    [SerializeField] private TextMeshProUGUI blindRewardText;
    [SerializeField] private TextMeshProUGUI roundScoreText;
    [SerializeField] private TextMeshProUGUI handNameText;
    [SerializeField] private TextMeshProUGUI handLevelText;
    [SerializeField] private TextMeshProUGUI chipsText;
    [SerializeField] private TextMeshProUGUI multText;
    [SerializeField] private TextMeshProUGUI handsLeftText;
    [SerializeField] private TextMeshProUGUI discardsLeftText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI anteText;
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("Top and Bottom Bars")]
    //[SerializeField] private TextMeshProUGUI phaseText;
    //[SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI selectedCountText;
    [SerializeField] private TextMeshProUGUI handSizeText;
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private TextMeshProUGUI topDiscardText;

    [Header("Buttons")]
    [SerializeField] private Button playHandButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button sortByRankButton;
    [SerializeField] private Button sortBySuitButton;

    [Header("Card Areas")]
    [SerializeField] private RectTransform handArea;
    [SerializeField] private RectTransform playedHandArea;
    [SerializeField] private CardView cardViewPrefab;

    [Header("Round End Overlay")]
    [SerializeField] private GameObject roundEndOverlay;
    [SerializeField] private Image roundEndBannerImage;
    [SerializeField] private TextMeshProUGUI roundEndBannerText;
    [SerializeField] private TextMeshProUGUI roundEndSummaryText;
    [SerializeField] private TextMeshProUGUI roundEndDetailsText;
    [SerializeField] private Button newRunButton;
    [SerializeField] private TextMeshProUGUI newRunButtonLabel;
    [SerializeField] private Button exitButton;

    [Header("Debug")]
    [SerializeField] private bool useDebugHandScenario = false;
    [SerializeField] private DebugHandScenario debugHandScenario = DebugHandScenario.None;

    private RoundPresenter _roundPresenter;
    private RoundState _roundState;

    private void Awake() {
        ResolveRoundEndOverlayReferences();
        RegisterButtonListeners();
    }

    private void Start() {
        _roundPresenter = new RoundPresenter();
        _roundState = CreateInitialState();

        Render(_roundState);
    }

    private void OnDestroy() {
        UnregisterButtonListeners();
    }

    private RoundState CreateInitialState() {
        var debugHand = GetDebugHand();

        return RoundState.CreateInitial(initialHandCards: debugHand);
    }

    public void OnPlayHandButtonClicked() {
        _roundState = _roundState.PlaySelectedCards();
        Render(_roundState);
    }

    public void OnDiscardButtonClicked() {
        _roundState = _roundState.DiscardCards();
        Render(_roundState);
    }

    public void OnSortByRankButtonClicked() {
        _roundState = _roundState.SortHandByRank();
        Render(_roundState);
    }

    public void OnSortBySuitButtonClicked() {
        _roundState = _roundState.SortHandBySuit();
        Render(_roundState);
    }

    private void Render(RoundState roundState) {
        var viewModel = _roundPresenter.Present(roundState);

        blindTitleText.text = viewModel.BlindTitleText;
        blindDescriptionText.text = viewModel.BlindDescriptionText;
        blindRequirementText.text = viewModel.BlindRequirementText;
        blindRewardText.text = viewModel.BlindRewardText;
        roundScoreText.text = viewModel.RoundScoreText;
        handNameText.text = viewModel.HandNameText;
        handLevelText.text = viewModel.HandLevelText;
        chipsText.text = viewModel.ChipsText;
        multText.text = viewModel.MultText;
        handsLeftText.text = viewModel.HandsLeftText;
        discardsLeftText.text = viewModel.DiscardsLeftText;
        moneyText.text = viewModel.MoneyText;
        anteText.text = viewModel.AnteText;
        roundText.text = viewModel.RoundText;
        //phaseText.text = viewModel.PhaseText;
        //statusText.text = viewModel.StatusText;
        selectedCountText.text = viewModel.SelectedCountText;
        handSizeText.text = viewModel.HandSizeText;
        deckCountText.text = viewModel.DeckCountText;
        topDiscardText.text = viewModel.TopDiscardText;

        playHandButton.interactable = viewModel.CanPlayHand;
        discardButton.interactable = viewModel.CanDiscard;
        sortByRankButton.interactable = viewModel.CanSort;
        sortBySuitButton.interactable = viewModel.CanSort;

        RenderHand(viewModel.HandCards);
        RenderPlayedCards(viewModel.PlayedCards);
        RenderRoundEndOverlay(viewModel);
    }

    private void RenderHand(IReadOnlyList<CardViewModel> handCards) {
        ClearCardArea(handArea);

        if (handCards.Count == 0) {
            return;
        }

        for (int i = 0; i < handCards.Count; i++) {
            var cardView = Instantiate(cardViewPrefab, handArea);
            cardView.Bind(handCards[i]);
            cardView.OnCardSelected += OnCardSelected;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(handArea);
    }

    private void RenderPlayedCards(IReadOnlyList<CardViewModel> playedCards) {
        ClearCardArea(playedHandArea);
        playedHandArea.gameObject.SetActive(playedCards.Count > 0);

        if (playedCards.Count == 0) {
            return;
        }

        for (int i = 0; i < playedCards.Count; i++) {
            var cardView = Instantiate(cardViewPrefab, playedHandArea);
            cardView.Bind(playedCards[i]);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(playedHandArea);
    }

    private void OnCardSelected(int index) {
        _roundState = _roundState.ToggleCardSelection(index);
        Render(_roundState);
    }

    private void HandlePrimaryRoundEndAction() {
        _roundState = _roundState != null && _roundState.CanAdvanceToNextBlind
            ? _roundState.StartNextBlind(initialHandCards: GetDebugHand())
            : CreateInitialState();

        Render(_roundState);
    }

    private void ExitRun() {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void RenderRoundEndOverlay(RoundViewModel viewModel) {
        if (roundEndOverlay == null) {
            return;
        }

        roundEndOverlay.SetActive(viewModel.ShowRoundEndOverlay);

        if (!viewModel.ShowRoundEndOverlay) {
            return;
        }

        if (roundEndBannerImage != null) {
            roundEndBannerImage.color = viewModel.IsWinningRoundEnd ? OverlayWinColor : OverlayLossColor;
        }

        if (roundEndBannerText != null) {
            roundEndBannerText.text = viewModel.RoundEndBannerText;
        }

        if (roundEndSummaryText != null) {
            roundEndSummaryText.text = viewModel.RoundEndSummaryText;
        }

        if (roundEndDetailsText != null) {
            roundEndDetailsText.text = viewModel.RoundEndDetailsText;
        }

        if (newRunButtonLabel != null) {
            newRunButtonLabel.text = viewModel.RoundEndPrimaryActionText;
        }
    }

    private void ResolveRoundEndOverlayReferences() {
        if (roundEndOverlay == null) {
            return;
        }

        roundEndBannerImage ??= FindOverlayComponent<Image>("Panel/Banner");
        roundEndBannerText ??= FindOverlayComponent<TextMeshProUGUI>("Panel/Banner/BannerText");
        roundEndSummaryText ??= FindOverlayComponent<TextMeshProUGUI>("Panel/SummaryText");
        roundEndDetailsText ??= FindOverlayComponent<TextMeshProUGUI>("Panel/DetailsText");
        newRunButton ??= FindOverlayComponent<Button>("Panel/NewRunButton");
        newRunButtonLabel ??= FindOverlayComponent<TextMeshProUGUI>("Panel/NewRunButton/Label");
        exitButton ??= FindOverlayComponent<Button>("Panel/ExitButton");
    }

    private void RegisterButtonListeners() {
        if (newRunButton != null) {
            newRunButton.onClick.AddListener(HandlePrimaryRoundEndAction);
        }

        if (exitButton != null) {
            exitButton.onClick.AddListener(ExitRun);
        }

        if (sortByRankButton != null) {
            sortByRankButton.onClick.AddListener(OnSortByRankButtonClicked);
        }

        if (sortBySuitButton != null) {
            sortBySuitButton.onClick.AddListener(OnSortBySuitButtonClicked);
        }
    }

    private void UnregisterButtonListeners() {
        if (newRunButton != null) {
            newRunButton.onClick.RemoveListener(HandlePrimaryRoundEndAction);
        }

        if (exitButton != null) {
            exitButton.onClick.RemoveListener(ExitRun);
        }

        if (sortByRankButton != null) {
            sortByRankButton.onClick.RemoveListener(OnSortByRankButtonClicked);
        }

        if (sortBySuitButton != null) {
            sortBySuitButton.onClick.RemoveListener(OnSortBySuitButtonClicked);
        }
    }

    private T FindOverlayComponent<T>(string relativePath) where T : Component {
        if (roundEndOverlay == null) {
            return null;
        }

        Transform target = roundEndOverlay.transform.Find(relativePath);
        return target != null ? target.GetComponent<T>() : null;
    }

    private IReadOnlyList<CardData> GetDebugHand() {
        return useDebugHandScenario && debugHandScenario != DebugHandScenario.None
            ? DebugHandFactory.Create(debugHandScenario)
            : null;
    }

    private static void ClearCardArea(RectTransform cardArea) {
        for (int i = cardArea.childCount - 1; i >= 0; i--) {
            if (Application.isPlaying) {
                Object.Destroy(cardArea.GetChild(i).gameObject);
            } else {
                Object.DestroyImmediate(cardArea.GetChild(i).gameObject);
            }
        }
    }
}
