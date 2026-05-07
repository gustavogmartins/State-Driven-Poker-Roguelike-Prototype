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
    [SerializeField] private RectTransform upperGlassArea;
    [SerializeField] private RectTransform handArea;
    [SerializeField] private RectTransform playedHandArea;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private int handSlotCount = 8;
    [SerializeField] private int playedCardSlotCount = 5;

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

    private RoundPresenter _roundPresenter;
    private RunState _runState;
    private int _selectedOwnedJokerIndex = -1;
    private readonly List<RectTransform> _handSlots = new();
    private readonly List<RectTransform> _playedCardSlots = new();
    private readonly Dictionary<CardData, CardView> _handCardViewsByCard = new();

    private void Awake() {
        ResolveRoundEndOverlayReferences();
        ResolveShopOverlayReferences();
        ResolveMainAreaReferences();
        EnsureCardSlots();
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
        NormalizeSelectedOwnedJoker(runState);

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
        //topDiscardText.text = viewModel.TopDiscardText;

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
            ownedJokers[i].IsSellSelected = i == _selectedOwnedJokerIndex;
            var cardView = Instantiate(cardViewPrefab, upperGlassArea);
            cardView.Bind(ownedJokers[i]);
            cardView.OnCardSelected += HandleOwnedJokerSelected;
            cardView.OnSellRequested += HandleOwnedJokerSell;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(upperGlassArea);
    }

    private void RenderHand(IReadOnlyList<CardViewModel> handCards) {
        EnsureCardSlots();
        var activeCards = new HashSet<CardData>();

        if (handCards.Count == 0) {
            ClearInactiveHandCards(activeCards);
            return;
        }

        for (int i = 0; i < handCards.Count; i++) {
            CardData cardData = _runState.CurrentRound.HandCards[i];
            activeCards.Add(cardData);

            if (!_handCardViewsByCard.TryGetValue(cardData, out CardView cardView) || cardView == null) {
                cardView = Instantiate(cardViewPrefab, _handSlots[i]);
                _handCardViewsByCard[cardData] = cardView;
            }

            ParentCardToSlot(cardView, _handSlots[i]);
            cardView.Bind(handCards[i]);
            cardView.OnCardSelected -= OnCardSelected;
            cardView.OnCardSelected += OnCardSelected;
        }

        ClearInactiveHandCards(activeCards);
        LayoutRebuilder.ForceRebuildLayoutImmediate(handArea);
    }

    private void RenderPlayedCards(IReadOnlyList<CardViewModel> playedCards) {
        EnsureCardSlots();
        ClearCardsInSlots(_playedCardSlots);
        playedHandArea.gameObject.SetActive(playedCards.Count > 0);

        if (playedCards.Count == 0) {
            return;
        }

        for (int i = 0; i < playedCards.Count; i++) {
            CardView cardView = Instantiate(cardViewPrefab, _playedCardSlots[i]);
            ParentCardToSlot(cardView, _playedCardSlots[i]);
            cardView.OnCardSelected -= OnCardSelected;
            cardView.Bind(playedCards[i]);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(playedHandArea);
    }

    private void OnCardSelected(int index) {
        _runState = _runState.ToggleCardSelection(index);
        Render(_runState);
    }

    private void HandlePrimaryRoundEndAction() {
        _selectedOwnedJokerIndex = -1;
        _runState = _runState != null && _runState.CanEnterShop
            ? _runState.EnterShop()
            : CreateInitialState();

        Render(_runState);
    }

    private void HandleShopContinueAction() {
        if (_runState == null) {
            return;
        }

        _selectedOwnedJokerIndex = -1;
        _runState = _runState.LeaveShop(initialHandCards: GetDebugHand());
        Render(_runState);
    }

    private void HandleShopOfferSelected(int index) {
        if (_runState == null) {
            return;
        }

        _runState = _runState.SelectShopOffer(index);
        Render(_runState);
    }

    private void HandleShopOfferBuy(int index) {
        if (_runState == null) {
            return;
        }

        _selectedOwnedJokerIndex = -1;
        _runState = _runState.BuyShopOffer(index);
        Render(_runState);
    }

    private void HandleShopRerollAction() {
        if (_runState == null) {
            return;
        }

        _selectedOwnedJokerIndex = -1;
        _runState = _runState.RerollShop();
        Render(_runState);
    }

    private void HandleOwnedJokerSelected(int index) {
        if (_runState == null || !_runState.CanSellOwnedJoker(index)) {
            return;
        }

        _selectedOwnedJokerIndex = index;
        Render(_runState);
    }

    private void HandleOwnedJokerSell(int index) {
        if (_runState == null) {
            return;
        }

        _runState = _runState.SellOwnedJoker(index);
        _selectedOwnedJokerIndex = -1;
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
        if (upperGlassArea != null) {
            return;
        }

        GameObject upperGlassObject = GameObject.Find("Canvas/HudRoot/MainArea/UpperGlass");
        if (upperGlassObject != null) {
            upperGlassArea = upperGlassObject.GetComponent<RectTransform>();
        }
    }

    private void EnsureCardSlots() {
        int resolvedHandSlotCount = Mathf.Max(handSlotCount, _runState?.CurrentRound.MaxHandSize ?? handSlotCount);
        EnsureSlots(handArea, _handSlots, resolvedHandSlotCount, "HandSlot");
        EnsureSlots(playedHandArea, _playedCardSlots, playedCardSlotCount, "PlayedCardSlot");
    }

    private void EnsureSlots(RectTransform area, List<RectTransform> slots, int slotCount, string slotNamePrefix) {
        if (area == null || cardViewPrefab == null) {
            return;
        }

        for (int i = slots.Count - 1; i >= 0; i--) {
            if (slots[i] == null || slots[i].parent != area) {
                slots.RemoveAt(i);
            }
        }

        RectTransform prefabTransform = cardViewPrefab.transform as RectTransform;
        Vector2 slotSize = prefabTransform != null ? prefabTransform.sizeDelta : new Vector2(165f, 230f);

        while (slots.Count < slotCount) {
            var slotObject = new GameObject($"{slotNamePrefix}{slots.Count + 1}", typeof(RectTransform), typeof(LayoutElement));
            RectTransform slot = (RectTransform)slotObject.transform;
            slot.SetParent(area, false);
            slot.anchorMin = new Vector2(0.5f, 0.5f);
            slot.anchorMax = new Vector2(0.5f, 0.5f);
            slot.pivot = new Vector2(0.5f, 0.5f);
            slot.sizeDelta = slotSize;

            LayoutElement layoutElement = slotObject.GetComponent<LayoutElement>();
            layoutElement.minWidth = slotSize.x;
            layoutElement.minHeight = slotSize.y;
            layoutElement.preferredWidth = slotSize.x;
            layoutElement.preferredHeight = slotSize.y;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;

            slots.Add(slot);
        }

        for (int i = 0; i < slots.Count; i++) {
            slots[i].gameObject.SetActive(i < slotCount);
            slots[i].SetSiblingIndex(i);
        }
    }

    private void ParentCardToSlot(CardView cardView, RectTransform slot) {
        if (cardView == null || slot == null) {
            return;
        }

        RectTransform cardTransform = cardView.transform as RectTransform;
        cardTransform.SetParent(slot, false);
        cardTransform.anchorMin = new Vector2(0.5f, 0.5f);
        cardTransform.anchorMax = new Vector2(0.5f, 0.5f);
        cardTransform.pivot = new Vector2(0.5f, 0.5f);
        cardTransform.anchoredPosition = Vector2.zero;
        cardTransform.localRotation = Quaternion.identity;
        cardTransform.localScale = Vector3.one;

        if (cardViewPrefab.transform is RectTransform prefabTransform) {
            cardTransform.sizeDelta = prefabTransform.sizeDelta;
        }
    }

    private void ClearInactiveHandCards(HashSet<CardData> activeCards) {
        var staleCards = new List<CardData>();

        foreach (var pair in _handCardViewsByCard) {
            if (!activeCards.Contains(pair.Key)) {
                if (pair.Value != null) {
                    Destroy(pair.Value.gameObject);
                }

                staleCards.Add(pair.Key);
            }
        }

        for (int i = 0; i < staleCards.Count; i++) {
            _handCardViewsByCard.Remove(staleCards[i]);
        }
    }

    private static void ClearCardsInSlots(IReadOnlyList<RectTransform> slots) {
        for (int i = 0; i < slots.Count; i++) {
            RectTransform slot = slots[i];
            if (slot == null) {
                continue;
            }

            ClearCardArea(slot);
        }
    }

    private static CardView GetCardInSlot(RectTransform slot) {
        if (slot == null || slot.childCount == 0) {
            return null;
        }

        return slot.GetChild(0).GetComponent<CardView>();
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

    private void NormalizeSelectedOwnedJoker(RunState runState) {
        if (runState == null || !runState.IsInShop || _selectedOwnedJokerIndex >= runState.OwnedJokers.Count) {
            _selectedOwnedJokerIndex = -1;
        }
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
