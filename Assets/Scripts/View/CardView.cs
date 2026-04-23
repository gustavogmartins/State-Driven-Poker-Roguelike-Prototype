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

    private CardViewModel _viewModel;
    private RectTransform _rectTransform;

    public event Action<int> OnCardSelected;

    private void Awake() {
        _rectTransform = (RectTransform)transform;
    }

    public void Bind(CardViewModel viewModel) {
        _viewModel = viewModel;

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

        cardImage.color = viewModel.IsSelected
            ? new Color32(255, 248, 221, 255)
            : Color.white;

        selectionGlow.enabled = viewModel.IsSelected;
        cardImage.raycastTarget = viewModel.IsInteractable;
        _rectTransform.localScale = viewModel.IsSelected ? new Vector3(1.04f, 1.04f, 1f) : Vector3.one;
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (_viewModel == null || !_viewModel.IsInteractable || _viewModel.Index < 0) {
            return;
        }

        OnCardSelected?.Invoke(_viewModel.Index);
    }
}
