using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerUpHandler {
    [SerializeField] private Image cardImage;
    [SerializeField] private Image selectionGlow;
    [SerializeField] private TextMeshProUGUI rankTopLeftText;
    [SerializeField] private TextMeshProUGUI suitTopLeftText;
    [SerializeField] private TextMeshProUGUI rankBottomRightText;
    [SerializeField] private TextMeshProUGUI suitBottomRightText;
    [SerializeField] private TextMeshProUGUI centerSuitText;
    [SerializeField] private Button sellJokerButton;
    [SerializeField] private TextMeshProUGUI sellJokerButtonLabel;

    private CardViewModel _viewModel;
    private RectTransform _rectTransform;
    private bool _isSellButtonListenerRegistered;

    public event Action<int> OnCardSelected;
    public event Action<int> OnSellRequested;

    private void Awake() {
        _rectTransform = (RectTransform)transform;
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

        if (_rectTransform == null) {
            _rectTransform = (RectTransform)transform;
        }

        rankTopLeftText.text = viewModel.RankText;
        suitTopLeftText.text = viewModel.SuitText;
        rankBottomRightText.text = viewModel.RankText;
        suitBottomRightText.text = viewModel.SuitText;
        centerSuitText.text = viewModel.SuitText;

        rankTopLeftText.color = viewModel.AccentColor;
        suitTopLeftText.color = viewModel.AccentColor;
        rankBottomRightText.color = viewModel.AccentColor;
        suitBottomRightText.color = viewModel.AccentColor;
        centerSuitText.color = viewModel.AccentColor;

        bool isVisuallySelected = viewModel.IsSelected || viewModel.IsSellSelected;

        cardImage.color = isVisuallySelected
            ? new Color32(255, 248, 221, 255)
            : Color.white;

        selectionGlow.enabled = isVisuallySelected;
        cardImage.raycastTarget = viewModel.IsInteractable;
        _rectTransform.localScale = isVisuallySelected ? new Vector3(1.04f, 1.04f, 1f) : Vector3.one;

        if (sellJokerButtonLabel != null) {
            sellJokerButtonLabel.text = string.IsNullOrWhiteSpace(viewModel.SellButtonText)
                ? "Sell"
                : viewModel.SellButtonText;
        }

        if (sellJokerButton != null) {
            sellJokerButton.gameObject.SetActive(viewModel.CanSell && viewModel.IsSellSelected);
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
}
