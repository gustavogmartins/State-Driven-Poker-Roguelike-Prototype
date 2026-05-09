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
    [SerializeField] private RectTransform upperGlassArea;

    [SerializeField] private RectTransform handArea;
    [SerializeField] private RectTransform playedHandArea;
    [SerializeField] private RectTransform discardedCardsArea;
    [SerializeField] private RoundBoardRenderer boardRenderer;
    [SerializeField] private RoundAnimationController animationController;
    [SerializeField] private CardViewPool cardViewPool;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private int handSlotCount = 8;
    [SerializeField] private int playedCardSlotCount = 5;

    [Header("Round End Overlay")]
    [SerializeField] private GameObject roundEndOverlay;

    [SerializeField] private Image roundEndBannerImage;
    [SerializeField] private Button roundEndBannerButton;
    [SerializeField] private TextMeshProUGUI roundEndBannerText;
    [SerializeField] private TextMeshProUGUI roundEndSummaryText;
    [SerializeField] private TextMeshProUGUI roundEndDetailsText;
    [SerializeField] private Button exitButton;

    [Header("Shop Overlay")]
    [SerializeField] private GameObject shopOverlay;

    [SerializeField] private TextMeshProUGUI shopBannerText;
    [SerializeField] private TextMeshProUGUI shopSummaryText;
    [SerializeField] private TextMeshProUGUI shopDetailsText;
    [SerializeField] private RectTransform offerSlotsContainer;
    [SerializeField] private ToggleGroup offerToggleGroup;
    [SerializeField] private OfferView offerPrefab;
    [SerializeField] private Button shopRerollButton;
    [SerializeField] private TextMeshProUGUI shopRerollButtonLabel;
    [SerializeField] private Button shopContinueButton;
    [SerializeField] private TextMeshProUGUI shopContinueButtonLabel;

    [Header("Debug")]
    [SerializeField] private bool useDebugHandScenario = false;

    [SerializeField] private DebugHandScenario debugHandScenario = DebugHandScenario.None;

    private readonly List<CardView> _ownedJokerCardViews = new();
    private RoundPresenter _roundPresenter;
    private GameStore _store;

    private void Awake() {
        ResolveRoundEndOverlayReferences();
        ResolveShopOverlayReferences();
        ResolveMainAreaReferences();
        EnsureBoardRenderer();
        RegisterButtonListeners();
    }

    private void Start() {
        _roundPresenter = new RoundPresenter();
        _store = new GameStore(CreateInitialState());
        _store.StateChanged += Render;

        Render(_store.State);
    }

    private void OnDestroy() {
        if (_store != null) {
            _store.StateChanged -= Render;
        }

        if (boardRenderer != null) {
            boardRenderer.CardSelected -= OnCardSelected;
            boardRenderer.ScoringPresentationFinished -= OnScoringPresentationFinished;
            boardRenderer.DiscardPresentationFinished -= OnDiscardPresentationFinished;
        }

        UnregisterButtonListeners();
    }

    private RunState CreateInitialState() {
        var debugHand = GetDebugHand();

        return RunState.CreateInitial(initialHandCards: debugHand);
    }

    public void OnPlayHandButtonClicked() {
        if (IsInputBlocked()) {
            return;
        }

        _store?.Dispatch(new PlaySelectedCardsAction());
    }

    public void OnDiscardButtonClicked() {
        if (IsInputBlocked()) {
            return;
        }

        _store?.Dispatch(new DiscardSelectedCardsAction());
    }

    public void OnSortByRankButtonClicked() {
        if (IsInputBlocked()) {
            return;
        }

        _store?.Dispatch(new SortHandByRankAction());
    }

    public void OnSortBySuitButtonClicked() {
        if (IsInputBlocked()) {
            return;
        }

        _store?.Dispatch(new SortHandBySuitAction());
    }

    private void Render(RunState runState) {
        var viewModel = _roundPresenter.Present(runState);

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
        selectedCountText.text = viewModel.SelectedCountText;
        handSizeText.text = viewModel.HandSizeText;
        deckCountText.text = viewModel.DeckCountText;

        playHandButton.interactable = viewModel.CanPlayHand;
        discardButton.interactable = viewModel.CanDiscard;
        sortByRankButton.interactable = viewModel.CanSort;
        sortBySuitButton.interactable = viewModel.CanSort;

        RenderOwnedJokers(viewModel.OwnedJokerCards);
        boardRenderer?.Render(viewModel, _ownedJokerCardViews);
        RenderRoundEndOverlay(viewModel);
        RenderShopOverlay(viewModel);
    }

    private void RenderOwnedJokers(IReadOnlyList<CardViewModel> ownedJokers) {
        if (upperGlassArea == null) {
            _ownedJokerCardViews.Clear();
            return;
        }

        _ownedJokerCardViews.Clear();
        ClearCardArea(upperGlassArea);

        for (int i = 0; i < ownedJokers.Count; i++) {
            CardViewModel ownedJoker = ownedJokers[i];
            var cardView = Instantiate(cardViewPrefab, upperGlassArea);
            cardView.Bind(ownedJoker);
            cardView.OnCardSelected += HandleOwnedJokerSelected;
            cardView.OnSellRequested += HandleOwnedJokerSell;
            _ownedJokerCardViews.Add(cardView);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(upperGlassArea);
    }

    private void OnCardSelected(int index) {
        if (IsInputBlocked()) {
            return;
        }

        _store?.Dispatch(new ToggleCardSelectionAction(index));
    }

    private void OnScoringPresentationFinished() {
        if (_store?.State?.CurrentRound.Phase != RoundPhase.Scoring) {
            return;
        }

        _store.Dispatch(new ScorePresentationFinishedAction());
    }

    private void OnDiscardPresentationFinished() {
        if (_store?.State?.CurrentRound.Phase != RoundPhase.Discarding) {
            return;
        }

        _store.Dispatch(new DiscardPresentationFinishedAction());
    }

    private void HandlePrimaryRoundEndAction() {
        _store?.Dispatch(new ContinueRoundEndAction(GetDebugHand()));
    }

    private void HandleShopContinueAction() {
        if (_store == null) {
            return;
        }

        _store.Dispatch(new ContinueShopAction(GetDebugHand()));
    }

    private void HandleShopOfferSelected(int index) {
        if (_store == null) {
            return;
        }

        _store.Dispatch(new SelectShopOfferAction(index));
    }

    private void HandleShopOfferBuy(int index) {
        if (_store == null) {
            return;
        }

        _store.Dispatch(new BuyShopOfferAction(index));
    }

    private void HandleShopRerollAction() {
        if (_store == null) {
            return;
        }

        _store.Dispatch(new RerollShopAction());
    }

    private void HandleOwnedJokerSelected(int index) {
        if (_store == null || !_store.State.CanSellOwnedJoker(index)) {
            return;
        }

        _store.Dispatch(new SelectOwnedJokerAction(index));
    }

    private void HandleOwnedJokerSell(int index) {
        if (_store == null) {
            return;
        }

        _store.Dispatch(new SellOwnedJokerAction(index));
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

        if (roundEndBannerButton != null) {
            roundEndBannerButton.interactable = viewModel.ShowRoundEndOverlay;
        }
    }

    private void RenderShopOverlay(RoundViewModel viewModel) {
        if (shopOverlay == null) {
            return;
        }

        shopOverlay.SetActive(viewModel.ShowShopOverlay);

        if (!viewModel.ShowShopOverlay) {
            return;
        }

        if (shopBannerText != null) {
            shopBannerText.text = viewModel.ShopBannerText;
        }

        if (shopSummaryText != null) {
            shopSummaryText.text = viewModel.ShopSummaryText;
        }

        if (shopDetailsText != null) {
            shopDetailsText.text = viewModel.ShopDetailsText;
        }

        RenderShopOfferSlots(viewModel.ShopOffers);

        if (shopRerollButton != null) {
            shopRerollButton.interactable = viewModel.CanRerollShop;
        }

        if (shopRerollButtonLabel != null) {
            shopRerollButtonLabel.text = viewModel.ShopRerollButtonText;
        }

        if (shopContinueButtonLabel != null) {
            shopContinueButtonLabel.text = viewModel.ShopPrimaryActionText;
        }
    }

    private void RenderShopOfferSlots(IReadOnlyList<ShopOfferViewModel> shopOffers) {
        if (!EnsureOfferSlotReferences()) {
            return;
        }

        ClearCardArea(offerSlotsContainer);

        for (int i = 0; i < shopOffers.Count; i++) {
            OfferView offerView = Instantiate(offerPrefab, offerSlotsContainer);
            offerView.SetToggleGroup(offerToggleGroup);
            offerView.Bind(shopOffers[i], HandleShopOfferSelected, HandleShopOfferBuy);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(offerSlotsContainer);
    }

    private bool EnsureOfferSlotReferences() {
        if (shopOverlay == null) {
            return false;
        }

        if (offerSlotsContainer == null) {
            Transform container = shopOverlay.transform.Find("Panel/OfferSlots");
            offerSlotsContainer = container != null ? container.GetComponent<RectTransform>() : null;
        }

        if (offerSlotsContainer == null) {
            return false;
        }

        if (offerToggleGroup == null) {
            offerToggleGroup = offerSlotsContainer.GetComponent<ToggleGroup>();
            if (offerToggleGroup == null) {
                offerToggleGroup = offerSlotsContainer.gameObject.AddComponent<ToggleGroup>();
            }
        }

        if (offerPrefab == null) {
#if UNITY_EDITOR
            offerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<OfferView>("Assets/Prefabs/Offer.prefab");
#endif
        }

        return offerPrefab != null;
    }

    private void ResolveRoundEndOverlayReferences() {
        if (roundEndOverlay == null) {
            return;
        }

        roundEndBannerImage ??= FindOverlayComponent<Image>("Panel/Banner");
        roundEndBannerButton ??= FindOverlayComponent<Button>("Panel/Banner");
        if (roundEndBannerButton == null && roundEndBannerImage != null) {
            roundEndBannerButton = roundEndBannerImage.GetComponent<Button>();
            if (roundEndBannerButton == null) {
                roundEndBannerButton = roundEndBannerImage.gameObject.AddComponent<Button>();
            }

            roundEndBannerButton.targetGraphic = roundEndBannerImage;
        }

        roundEndBannerText ??= FindOverlayComponent<TextMeshProUGUI>("Panel/Banner/BannerText");
        roundEndSummaryText ??= FindOverlayComponent<TextMeshProUGUI>("Panel/SummaryText");
        roundEndDetailsText ??= FindOverlayComponent<TextMeshProUGUI>("Panel/DetailsText");
        exitButton ??= FindOverlayComponent<Button>("Panel/ExitButton");
    }

    private void ResolveShopOverlayReferences() {
        if (shopOverlay == null) {
            return;
        }

        Image shopPanelImage = FindOverlayComponent<Image>(shopOverlay, "Panel");
        if (shopPanelImage != null) {
            shopPanelImage.raycastTarget = false;
        }

        shopBannerText ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/Banner/BannerText");
        shopSummaryText ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/SummaryText");
        shopDetailsText ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/DetailsText");
        offerSlotsContainer ??= FindOverlayComponent<RectTransform>(shopOverlay, "Panel/OfferSlots");
        if (offerSlotsContainer != null) {
            offerToggleGroup ??= offerSlotsContainer.GetComponent<ToggleGroup>();
        }

        shopRerollButton ??= FindOverlayComponent<Button>(shopOverlay, "Panel/RerollButton");
        shopRerollButtonLabel ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/RerollButton/Label");
        shopContinueButton ??= FindOverlayComponent<Button>(shopOverlay, "Panel/ContinueButton");
        shopContinueButtonLabel ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/ContinueButton/Label");
    }

    private void ResolveMainAreaReferences() {
        if (upperGlassArea != null && discardedCardsArea != null) {
            return;
        }

        if (upperGlassArea == null) {
            GameObject upperGlassObject = GameObject.Find("Canvas/HudRoot/MainArea/UpperGlass");
            if (upperGlassObject != null) {
                upperGlassArea = upperGlassObject.GetComponent<RectTransform>();
            }
        }

        if (discardedCardsArea == null) {
            GameObject discardedCardsObject = GameObject.Find("Canvas/HudRoot/MainArea/DiscardedCardsArea");
            if (discardedCardsObject != null) {
                discardedCardsArea = discardedCardsObject.GetComponent<RectTransform>();
            }
        }
    }

    private void EnsureBoardRenderer() {
        if (animationController == null) {
            animationController = GetComponent<RoundAnimationController>();
            if (animationController == null) {
                animationController = gameObject.AddComponent<RoundAnimationController>();
            }
        }

        if (cardViewPool == null) {
            cardViewPool = GetComponent<CardViewPool>();
            if (cardViewPool == null) {
                cardViewPool = gameObject.AddComponent<CardViewPool>();
            }
        }

        if (boardRenderer == null) {
            boardRenderer = GetComponent<RoundBoardRenderer>();
            if (boardRenderer == null) {
                boardRenderer = gameObject.AddComponent<RoundBoardRenderer>();
            }
        }

        cardViewPool.Configure(cardViewPrefab);
        animationController.ConfigureScoreTargets(chipsText, multText, roundScoreText);
        boardRenderer.Configure(
            cardViewPrefab,
            handArea,
            playedHandArea,
            discardedCardsArea,
            animationController,
            cardViewPool,
            handSlotCount,
            playedCardSlotCount);

        boardRenderer.CardSelected -= OnCardSelected;
        boardRenderer.CardSelected += OnCardSelected;
        boardRenderer.ScoringPresentationFinished -= OnScoringPresentationFinished;
        boardRenderer.ScoringPresentationFinished += OnScoringPresentationFinished;
        boardRenderer.DiscardPresentationFinished -= OnDiscardPresentationFinished;
        boardRenderer.DiscardPresentationFinished += OnDiscardPresentationFinished;
    }

    private bool IsInputBlocked() {
        return animationController != null && animationController.IsAnimating;
    }

    private void RegisterButtonListeners() {
        if (roundEndBannerButton != null) {
            roundEndBannerButton.onClick.AddListener(HandlePrimaryRoundEndAction);
        }

        if (exitButton != null) {
            exitButton.onClick.AddListener(ExitRun);
        }

        if (shopContinueButton != null) {
            shopContinueButton.onClick.AddListener(HandleShopContinueAction);
        }

        if (shopRerollButton != null) {
            shopRerollButton.onClick.AddListener(HandleShopRerollAction);
        }

        if (sortByRankButton != null) {
            sortByRankButton.onClick.AddListener(OnSortByRankButtonClicked);
        }

        if (sortBySuitButton != null) {
            sortBySuitButton.onClick.AddListener(OnSortBySuitButtonClicked);
        }
    }

    private void UnregisterButtonListeners() {
        if (roundEndBannerButton != null) {
            roundEndBannerButton.onClick.RemoveListener(HandlePrimaryRoundEndAction);
        }

        if (exitButton != null) {
            exitButton.onClick.RemoveListener(ExitRun);
        }

        if (shopContinueButton != null) {
            shopContinueButton.onClick.RemoveListener(HandleShopContinueAction);
        }

        if (shopRerollButton != null) {
            shopRerollButton.onClick.RemoveListener(HandleShopRerollAction);
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

    private static T FindOverlayComponent<T>(GameObject overlayRoot, string relativePath) where T : Component {
        if (overlayRoot == null) {
            return null;
        }

        Transform target = overlayRoot.transform.Find(relativePath);
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
