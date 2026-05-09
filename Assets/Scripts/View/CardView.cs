using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private RectTransform visualRoot;
    [SerializeField] private Image cardImage;
    [SerializeField] private Image selectionGlow;
    [SerializeField] private TextMeshProUGUI rankTopLeftText;
    [SerializeField] private TextMeshProUGUI suitTopLeftText;
    [SerializeField] private TextMeshProUGUI rankBottomRightText;
    [SerializeField] private TextMeshProUGUI suitBottomRightText;
    [SerializeField] private TextMeshProUGUI centerSuitText;
    [SerializeField] private Button sellJokerButton;
    [SerializeField] private TextMeshProUGUI sellJokerButtonLabel;
    [SerializeField] private JokerTooltipView jokerTooltipPrefab;
    [SerializeField] private Vector2 jokerTooltipOffset;

    private CardViewModel _viewModel;
    private RectTransform _rectTransform;
    private JokerTooltipView _jokerTooltip;
    private bool _isSellButtonListenerRegistered;

    public event Action<int> OnCardSelected;
    public event Action<int> OnSellRequested;
    public RectTransform RectTransform => ResolveRectTransform();
    public RectTransform VisualRoot => ResolveVisualRoot();

    private void Awake() {
        ResolveRectTransform();
        ResolveVisualRoot();
        ResolveSellButtonReferences();
        RegisterSellButtonListener();
    }

    private void OnDestroy() {
        UnregisterSellButtonListener();
    }

    public void Bind(CardViewModel viewModel) {
        _viewModel = viewModel;
        ResolveSellButtonReferences();
        RegisterSellButtonListener();

        ResolveRectTransform();

        rankTopLeftText.text = viewModel.RankText;
        suitTopLeftText.text = viewModel.SuitText;
        rankBottomRightText.text = viewModel.RankText;
        suitBottomRightText.text = viewModel.SuitText;
        centerSuitText.text = viewModel.SuitText;

        Color textColor = viewModel.IsDebuffed
            ? new Color32(96, 103, 107, 255)
            : viewModel.AccentColor;

        rankTopLeftText.color = textColor;
        suitTopLeftText.color = textColor;
        rankBottomRightText.color = textColor;
        suitBottomRightText.color = textColor;
        centerSuitText.color = textColor;

        bool isVisuallySelected = viewModel.IsSelected || viewModel.IsSellSelected;

        cardImage.color = isVisuallySelected
            ? new Color32(255, 248, 221, 255)
            : viewModel.IsDebuffed
                ? new Color32(174, 178, 180, 255)
                : Color.white;

        selectionGlow.enabled = isVisuallySelected;
        cardImage.raycastTarget = viewModel.IsInteractable || viewModel.HasTooltip;

        if (sellJokerButtonLabel != null) {
            sellJokerButtonLabel.text = string.IsNullOrWhiteSpace(viewModel.SellButtonText)
                ? "Sell"
                : viewModel.SellButtonText;
        }

        if (sellJokerButton != null) {
            sellJokerButton.gameObject.SetActive(viewModel.CanSell && viewModel.IsSellSelected);
        }

        BindJokerTooltip(viewModel);
    }

    public void ResetView() {
        _viewModel = null;
        OnCardSelected = null;
        OnSellRequested = null;
        HideJokerTooltip();

        RectTransform rectTransform = RectTransform;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;

        RectTransform resolvedVisualRoot = VisualRoot;
        resolvedVisualRoot.anchoredPosition = Vector2.zero;
        resolvedVisualRoot.localRotation = Quaternion.identity;
        resolvedVisualRoot.localScale = Vector3.one;

        if (selectionGlow != null) {
            selectionGlow.enabled = false;
        }

        if (sellJokerButton != null) {
            sellJokerButton.gameObject.SetActive(false);
        }

        if (cardImage != null) {
            cardImage.raycastTarget = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (_viewModel == null || !_viewModel.IsInteractable || _viewModel.Index < 0) {
            return;
        }

        if (sellJokerButton != null && eventData.pointerPress != null) {
            Button pressedButton = eventData.pointerPress.GetComponentInParent<Button>();
            if (pressedButton == sellJokerButton) {
                return;
            }
        }

        OnCardSelected?.Invoke(_viewModel.Index);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (_viewModel?.HasTooltip != true) {
            return;
        }

        ShowJokerTooltip();
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (_viewModel?.HasTooltip != true) {
            return;
        }

        HideJokerTooltip();
    }

    private void BindJokerTooltip(CardViewModel viewModel) {
        if (viewModel?.HasTooltip != true) {
            HideJokerTooltip();
            return;
        }

        JokerTooltipView tooltip = EnsureJokerTooltip();
        if (tooltip == null) {
            return;
        }

        tooltip.Bind(viewModel.TooltipTitleText, viewModel.TooltipBodyText, viewModel.AccentColor);
        PositionJokerTooltip(tooltip);
        tooltip.Hide();
    }

    private void ShowJokerTooltip() {
        JokerTooltipView tooltip = EnsureJokerTooltip();
        if (tooltip == null || _viewModel?.HasTooltip != true) {
            return;
        }

        tooltip.Bind(_viewModel.TooltipTitleText, _viewModel.TooltipBodyText, _viewModel.AccentColor);
        PositionJokerTooltip(tooltip);
        tooltip.Show();
        tooltip.transform.SetAsLastSibling();
    }

    private void HideJokerTooltip() {
        if (_jokerTooltip != null) {
            _jokerTooltip.Hide();
        }
    }

    private JokerTooltipView EnsureJokerTooltip() {
        if (_jokerTooltip != null) {
            return _jokerTooltip;
        }

        if (jokerTooltipPrefab == null) {
#if UNITY_EDITOR
            jokerTooltipPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<JokerTooltipView>("Assets/Prefabs/JokerTooltipPrefab.prefab");
#endif
        }

        if (jokerTooltipPrefab == null) {
            return null;
        }

        _jokerTooltip = Instantiate(jokerTooltipPrefab, transform);
        PositionJokerTooltip(_jokerTooltip);
        _jokerTooltip.Hide();
        return _jokerTooltip;
    }

    private void PositionJokerTooltip(JokerTooltipView tooltip) {
        if (tooltip == null) {
            return;
        }

        RectTransform tooltipTransform = tooltip.RectTransform;
        tooltipTransform.anchorMin = new Vector2(1f, 0.5f);
        tooltipTransform.anchorMax = new Vector2(1f, 0.5f);
        tooltipTransform.pivot = new Vector2(1f, 0.5f);
        tooltipTransform.anchoredPosition = jokerTooltipOffset;
    }

    private void ResolveSellButtonReferences() {
        if (sellJokerButton == null) {
            Transform buttonTransform = transform.Find("SellJokerButton");
            sellJokerButton = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
        }

        if (sellJokerButtonLabel == null && sellJokerButton != null) {
            sellJokerButtonLabel = sellJokerButton.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        }
    }

    private void RegisterSellButtonListener() {
        if (sellJokerButton != null && !_isSellButtonListenerRegistered) {
            sellJokerButton.onClick.AddListener(HandleSellClicked);
            _isSellButtonListenerRegistered = true;
        }
    }

    private void UnregisterSellButtonListener() {
        if (sellJokerButton != null && _isSellButtonListenerRegistered) {
            sellJokerButton.onClick.RemoveListener(HandleSellClicked);
            _isSellButtonListenerRegistered = false;
        }
    }

    private void HandleSellClicked() {
        if (_viewModel == null || !_viewModel.CanSell || _viewModel.Index < 0) {
            return;
        }

        OnSellRequested?.Invoke(_viewModel.Index);
    }

    private RectTransform ResolveRectTransform() {
        if (_rectTransform == null) {
            _rectTransform = (RectTransform)transform;
        }

        return _rectTransform;
    }

    private RectTransform ResolveVisualRoot() {
        if (visualRoot == null) {
            Transform visualRootTransform = transform.Find("VisualRoot");
            visualRoot = visualRootTransform != null
                ? visualRootTransform.GetComponent<RectTransform>()
                : RectTransform;
        }

        return visualRoot;
    }
}
