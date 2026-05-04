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
    private static readonly Color32 ShopSlotBaseColor = new(24, 31, 35, 235);
    private static readonly Color32 ShopSlotSelectedColor = new(42, 54, 61, 245);
    private static readonly Color32 ShopSlotBoughtColor = new(35, 44, 48, 210);

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
    [SerializeField] private RectTransform upperGlassArea;
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

    [Header("Shop Overlay")]
    [SerializeField] private GameObject shopOverlay;
    [SerializeField] private TextMeshProUGUI shopBannerText;
    [SerializeField] private TextMeshProUGUI shopSummaryText;
    [SerializeField] private TextMeshProUGUI shopDetailsText;
    [SerializeField] private TextMeshProUGUI shopOffersText;
    [SerializeField] private Button shopPreviousOfferButton;
    [SerializeField] private Button shopNextOfferButton;
    [SerializeField] private Button shopBuyButton;
    [SerializeField] private TextMeshProUGUI shopBuyButtonLabel;
    [SerializeField] private Button shopRerollButton;
    [SerializeField] private TextMeshProUGUI shopRerollButtonLabel;
    [SerializeField] private Button shopContinueButton;
    [SerializeField] private TextMeshProUGUI shopContinueButtonLabel;

    [Header("Debug")]
    [SerializeField] private bool useDebugHandScenario = false;
    [SerializeField] private DebugHandScenario debugHandScenario = DebugHandScenario.None;

    private RoundPresenter _roundPresenter;
    private RunState _runState;
    private RectTransform _shopOffersContainer;
    private readonly List<ShopOfferSlotViews> _shopOfferSlots = new();

    private void Awake() {
        ResolveRoundEndOverlayReferences();
        ResolveShopOverlayReferences();
        ResolveMainAreaReferences();
        RegisterButtonListeners();
    }

    private void Start() {
        _roundPresenter = new RoundPresenter();
        _runState = CreateInitialState();

        Render(_runState);
    }

    private void OnDestroy() {
        UnregisterButtonListeners();
    }

    private RunState CreateInitialState() {
        var debugHand = GetDebugHand();

        return RunState.CreateInitial(initialHandCards: debugHand);
    }

    public void OnPlayHandButtonClicked() {
        _runState = _runState.PlaySelectedCards();
        Render(_runState);
    }

    public void OnDiscardButtonClicked() {
        _runState = _runState.DiscardCards();
        Render(_runState);
    }

    public void OnSortByRankButtonClicked() {
        _runState = _runState.SortHandByRank();
        Render(_runState);
    }

    public void OnSortBySuitButtonClicked() {
        _runState = _runState.SortHandBySuit();
        Render(_runState);
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

        RenderOwnedJokers(viewModel.OwnedJokerCards);
        RenderHand(viewModel.HandCards);
        RenderPlayedCards(viewModel.PlayedCards);
        RenderRoundEndOverlay(viewModel);
        RenderShopOverlay(viewModel);
    }

    private void RenderOwnedJokers(IReadOnlyList<CardViewModel> ownedJokers) {
        if (upperGlassArea == null) {
            return;
        }

        ClearCardArea(upperGlassArea);

        for (int i = 0; i < ownedJokers.Count; i++) {
            var cardView = Instantiate(cardViewPrefab, upperGlassArea);
            cardView.Bind(ownedJokers[i]);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(upperGlassArea);
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
        _runState = _runState.ToggleCardSelection(index);
        Render(_runState);
    }

    private void HandlePrimaryRoundEndAction() {
        _runState = _runState != null && _runState.CanEnterShop
            ? _runState.EnterShop()
            : CreateInitialState();

        Render(_runState);
    }

    private void HandleShopContinueAction() {
        if (_runState == null) {
            return;
        }

        _runState = _runState.LeaveShop(initialHandCards: GetDebugHand());
        Render(_runState);
    }

    private void HandleShopBuyAction() {
        if (_runState == null) {
            return;
        }

        _runState = _runState.BuySelectedShopOffer();
        Render(_runState);
    }

    private void HandlePreviousShopOfferAction() {
        if (_runState == null) {
            return;
        }

        _runState = _runState.SelectPreviousShopOffer();
        Render(_runState);
    }

    private void HandleNextShopOfferAction() {
        if (_runState == null) {
            return;
        }

        _runState = _runState.SelectNextShopOffer();
        Render(_runState);
    }

    private void HandleShopRerollAction() {
        if (_runState == null) {
            return;
        }

        _runState = _runState.RerollShop();
        Render(_runState);
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

        if (shopOffersText != null) {
            shopOffersText.text = viewModel.ShopOffersText;
        }

        RenderShopOfferSlots(viewModel.ShopOffers);

        if (shopBuyButton != null) {
            shopBuyButton.interactable = viewModel.CanBuySelectedShopOffer;
        }

        if (shopBuyButtonLabel != null) {
            shopBuyButtonLabel.text = viewModel.ShopBuyButtonText;
        }

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
        if (!EnsureShopOfferSlots(shopOffers.Count)) {
            return;
        }

        if (shopOffersText != null) {
            shopOffersText.gameObject.SetActive(false);
        }

        for (int i = 0; i < _shopOfferSlots.Count; i++) {
            ShopOfferSlotViews slot = _shopOfferSlots[i];
            bool isActive = i < shopOffers.Count;
            slot.Root.SetActive(isActive);

            if (!isActive) {
                continue;
            }

            ShopOfferViewModel offer = shopOffers[i];
            slot.Background.color = offer.IsPurchased
                ? ShopSlotBoughtColor
                : offer.IsSelected
                    ? ShopSlotSelectedColor
                    : ShopSlotBaseColor;
            slot.Accent.color = offer.AccentColor;
            slot.TitleText.text = offer.TitleText;
            slot.CostText.text = offer.CostText;
            slot.DescriptionText.text = offer.DescriptionText;
            slot.StatusText.text = offer.StatusText;
            slot.StatusText.color = offer.IsPurchased
                ? new Color32(166, 181, 184, 255)
                : offer.CanBuy
                    ? new Color32(244, 219, 118, 255)
                    : new Color32(220, 96, 96, 255);
            slot.SelectedFrame.enabled = offer.IsSelected;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_shopOffersContainer);
    }

    private bool EnsureShopOfferSlots(int slotCount) {
        if (shopOverlay == null) {
            return false;
        }

        if (_shopOffersContainer == null) {
            _shopOffersContainer = CreateShopOffersContainer();
        }

        if (_shopOffersContainer == null) {
            return false;
        }

        while (_shopOfferSlots.Count < slotCount) {
            _shopOfferSlots.Add(CreateShopOfferSlot(_shopOffersContainer, _shopOfferSlots.Count));
        }

        return true;
    }

    private RectTransform CreateShopOffersContainer() {
        RectTransform sourceRect = shopOffersText != null
            ? shopOffersText.rectTransform
            : null;
        Transform panel = shopOverlay.transform.Find("Panel");
        if (panel == null) {
            return null;
        }

        var containerObject = new GameObject("OfferSlots", typeof(RectTransform));
        containerObject.transform.SetParent(panel, false);

        var rectTransform = containerObject.GetComponent<RectTransform>();
        if (sourceRect != null) {
            rectTransform.anchorMin = sourceRect.anchorMin;
            rectTransform.anchorMax = sourceRect.anchorMax;
            rectTransform.anchoredPosition = sourceRect.anchoredPosition;
            rectTransform.sizeDelta = sourceRect.sizeDelta;
            rectTransform.pivot = sourceRect.pivot;
        } else {
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(30f, -355f);
            rectTransform.sizeDelta = new Vector2(700f, 180f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
        }

        var layoutGroup = containerObject.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 12f;
        layoutGroup.padding = new RectOffset(0, 0, 0, 0);
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = true;

        return rectTransform;
    }

    private static ShopOfferSlotViews CreateShopOfferSlot(RectTransform parent, int index) {
        var root = new GameObject($"OfferSlot{index + 1}", typeof(RectTransform));
        root.transform.SetParent(parent, false);

        var rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(220f, 180f);

        var background = root.AddComponent<Image>();
        background.color = ShopSlotBaseColor;
        background.raycastTarget = false;

        var selectedFrame = root.AddComponent<Outline>();
        selectedFrame.effectColor = new Color32(244, 219, 118, 255);
        selectedFrame.effectDistance = new Vector2(3f, -3f);
        selectedFrame.enabled = false;

        var verticalLayout = root.AddComponent<VerticalLayoutGroup>();
        verticalLayout.padding = new RectOffset(10, 10, 10, 10);
        verticalLayout.spacing = 5f;
        verticalLayout.childControlWidth = true;
        verticalLayout.childControlHeight = false;
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.childForceExpandHeight = false;

        var layoutElement = root.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 220f;
        layoutElement.preferredHeight = 180f;
        layoutElement.flexibleWidth = 1f;

        Image accent = CreateAccent(root.transform);
        TextMeshProUGUI titleText = CreateSlotText(root.transform, "Title", 20f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TextMeshProUGUI costText = CreateSlotText(root.transform, "Cost", 18f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TextMeshProUGUI descriptionText = CreateSlotText(root.transform, "Description", 15f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        TextMeshProUGUI statusText = CreateSlotText(root.transform, "Status", 15f, FontStyles.Bold, TextAlignmentOptions.BottomLeft);

        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 14f;
        titleText.fontSizeMax = 20f;
        descriptionText.enableAutoSizing = true;
        descriptionText.fontSizeMin = 11f;
        descriptionText.fontSizeMax = 15f;

        return new ShopOfferSlotViews(root, background, selectedFrame, accent, titleText, costText, descriptionText, statusText);
    }

    private static Image CreateAccent(Transform parent) {
        var accentObject = new GameObject("Accent", typeof(RectTransform));
        accentObject.transform.SetParent(parent, false);

        var rectTransform = accentObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0f, 5f);

        var image = accentObject.AddComponent<Image>();
        image.raycastTarget = false;

        var layoutElement = accentObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 5f;

        return image;
    }

    private static TextMeshProUGUI CreateSlotText(
        Transform parent,
        string name,
        float fontSize,
        FontStyles fontStyle,
        TextAlignmentOptions alignment) {
        var textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;

        var layoutElement = textObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = name switch {
            "Title" => 28f,
            "Cost" => 24f,
            "Description" => 78f,
            _ => 24f
        };

        return text;
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

    private void ResolveShopOverlayReferences() {
        if (shopOverlay == null) {
            return;
        }

        shopBannerText ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/Banner/BannerText");
        shopSummaryText ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/SummaryText");
        shopDetailsText ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/DetailsText");
        shopOffersText ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/OffersText");
        shopPreviousOfferButton ??= FindOverlayComponent<Button>(shopOverlay, "Panel/PreviousOfferButton");
        shopNextOfferButton ??= FindOverlayComponent<Button>(shopOverlay, "Panel/NextOfferButton");
        shopBuyButton ??= FindOverlayComponent<Button>(shopOverlay, "Panel/BuyOfferButton");
        shopBuyButtonLabel ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/BuyOfferButton/Label");
        shopRerollButton ??= FindOverlayComponent<Button>(shopOverlay, "Panel/RerollButton");
        shopRerollButtonLabel ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/RerollButton/Label");
        shopContinueButton ??= FindOverlayComponent<Button>(shopOverlay, "Panel/ContinueButton");
        shopContinueButtonLabel ??= FindOverlayComponent<TextMeshProUGUI>(shopOverlay, "Panel/ContinueButton/Label");
    }

    private void ResolveMainAreaReferences() {
        if (upperGlassArea != null) {
            return;
        }

        GameObject upperGlassObject = GameObject.Find("Canvas/HudRoot/MainArea/UpperGlass");
        if (upperGlassObject != null) {
            upperGlassArea = upperGlassObject.GetComponent<RectTransform>();
        }
    }

    private void RegisterButtonListeners() {
        if (newRunButton != null) {
            newRunButton.onClick.AddListener(HandlePrimaryRoundEndAction);
        }

        if (exitButton != null) {
            exitButton.onClick.AddListener(ExitRun);
        }

        if (shopContinueButton != null) {
            shopContinueButton.onClick.AddListener(HandleShopContinueAction);
        }

        if (shopBuyButton != null) {
            shopBuyButton.onClick.AddListener(HandleShopBuyAction);
        }

        if (shopPreviousOfferButton != null) {
            shopPreviousOfferButton.onClick.AddListener(HandlePreviousShopOfferAction);
        }

        if (shopNextOfferButton != null) {
            shopNextOfferButton.onClick.AddListener(HandleNextShopOfferAction);
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
        if (newRunButton != null) {
            newRunButton.onClick.RemoveListener(HandlePrimaryRoundEndAction);
        }

        if (exitButton != null) {
            exitButton.onClick.RemoveListener(ExitRun);
        }

        if (shopContinueButton != null) {
            shopContinueButton.onClick.RemoveListener(HandleShopContinueAction);
        }

        if (shopBuyButton != null) {
            shopBuyButton.onClick.RemoveListener(HandleShopBuyAction);
        }

        if (shopPreviousOfferButton != null) {
            shopPreviousOfferButton.onClick.RemoveListener(HandlePreviousShopOfferAction);
        }

        if (shopNextOfferButton != null) {
            shopNextOfferButton.onClick.RemoveListener(HandleNextShopOfferAction);
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

    private sealed class ShopOfferSlotViews {
        public GameObject Root { get; }
        public Image Background { get; }
        public Outline SelectedFrame { get; }
        public Image Accent { get; }
        public TextMeshProUGUI TitleText { get; }
        public TextMeshProUGUI CostText { get; }
        public TextMeshProUGUI DescriptionText { get; }
        public TextMeshProUGUI StatusText { get; }

        public ShopOfferSlotViews(
            GameObject root,
            Image background,
            Outline selectedFrame,
            Image accent,
            TextMeshProUGUI titleText,
            TextMeshProUGUI costText,
            TextMeshProUGUI descriptionText,
            TextMeshProUGUI statusText) {
            Root = root;
            Background = background;
            SelectedFrame = selectedFrame;
            Accent = accent;
            TitleText = titleText;
            CostText = costText;
            DescriptionText = descriptionText;
            StatusText = statusText;
        }
    }
}
