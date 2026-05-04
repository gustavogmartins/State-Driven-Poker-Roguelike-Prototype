using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View {
    public sealed class OfferView : MonoBehaviour {
        [SerializeField] private Image background;
        [SerializeField] private Outline selectedFrame;
        [SerializeField] private Image accent;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Toggle toggle;
        [SerializeField] private Button buyJokerButton;

        private static readonly Color32 BaseColor = new(42, 54, 61, 245);
        private static readonly Color32 SelectedColor = new(58, 72, 80, 255);
        private static readonly Color32 BoughtColor = new(35, 44, 48, 210);
        private static readonly Color32 AvailableStatusColor = new(244, 219, 118, 255);
        private static readonly Color32 BlockedStatusColor = new(220, 96, 96, 255);
        private static readonly Color32 BoughtStatusColor = new(166, 181, 184, 255);

        private int _index;
        private Action<int> _onSelected;
        private Action<int> _onBuy;
        private bool _canBuy;
        private bool _isPurchased;

        private void Awake() {
            ResolveReferences();
            RegisterListeners();
        }

        private void OnDestroy() {
            UnregisterListeners();
        }

        public void SetToggleGroup(ToggleGroup toggleGroup) {
            ResolveReferences();

            if (toggle != null) {
                toggle.group = toggleGroup;
            }
        }

        public void Bind(
            ShopOfferViewModel viewModel,
            Action<int> onSelected,
            Action<int> onBuy) {
            ResolveReferences();

            _index = viewModel.Index;
            _onSelected = onSelected;
            _onBuy = onBuy;
            _canBuy = viewModel.CanBuy;
            _isPurchased = viewModel.IsPurchased;

            if (titleText != null) {
                titleText.text = viewModel.TitleText;
            }

            if (rarityText != null) {
                rarityText.text = viewModel.RarityText;
                rarityText.color = viewModel.RarityColor;
            }

            if (costText != null) {
                costText.text = viewModel.CostText;
            }

            if (descriptionText != null) {
                descriptionText.text = viewModel.DescriptionText;
            }

            if (statusText != null) {
                statusText.text = viewModel.StatusText;
                statusText.color = viewModel.IsPurchased
                    ? BoughtStatusColor
                    : viewModel.CanBuy
                        ? AvailableStatusColor
                        : BlockedStatusColor;
            }

            if (accent != null) {
                accent.color = viewModel.AccentColor;
            }

            if (background != null) {
                background.color = viewModel.IsPurchased
                    ? BoughtColor
                    : viewModel.IsSelected
                        ? SelectedColor
                        : BaseColor;
            }

            if (selectedFrame != null) {
                selectedFrame.enabled = viewModel.IsSelected;
            }

            if (toggle != null) {
                toggle.SetIsOnWithoutNotify(viewModel.IsSelected);
            }

            UpdateBuyButton(viewModel.IsSelected);
        }

        private void ResolveReferences() {
            background ??= GetComponent<Image>();
            selectedFrame ??= GetComponent<Outline>();
            toggle ??= GetComponent<Toggle>();

            if (toggle == null) {
                toggle = gameObject.AddComponent<Toggle>();
            }

            if (background != null) {
                background.raycastTarget = true;
                toggle.targetGraphic = background;
            }

            accent ??= FindChildComponent<Image>("Accent");
            titleText ??= FindChildComponent<TextMeshProUGUI>("Title");
            rarityText ??= FindChildComponent<TextMeshProUGUI>("Rarity");
            costText ??= FindChildComponent<TextMeshProUGUI>("Cost");
            descriptionText ??= FindChildComponent<TextMeshProUGUI>("Description");
            statusText ??= FindChildComponent<TextMeshProUGUI>("Status");
            buyJokerButton ??= FindChildComponent<Button>("BuyJokerButton");
        }

        private T FindChildComponent<T>(string childName) where T : Component {
            Transform child = transform.Find(childName);
            return child != null ? child.GetComponent<T>() : null;
        }

        private void RegisterListeners() {
            if (toggle != null) {
                toggle.onValueChanged.AddListener(HandleToggleChanged);
            }

            if (buyJokerButton != null) {
                buyJokerButton.onClick.AddListener(HandleBuyClicked);
            }
        }

        private void UnregisterListeners() {
            if (toggle != null) {
                toggle.onValueChanged.RemoveListener(HandleToggleChanged);
            }

            if (buyJokerButton != null) {
                buyJokerButton.onClick.RemoveListener(HandleBuyClicked);
            }
        }

        private void HandleToggleChanged(bool isOn) {
            if (!isOn) {
                UpdateBuyButton(isSelected: false);
                return;
            }

            _onSelected?.Invoke(_index);
            UpdateBuyButton(isSelected: true);
        }

        private void HandleBuyClicked() {
            if (!_canBuy || _isPurchased) {
                return;
            }

            _onBuy?.Invoke(_index);
        }

        private void UpdateBuyButton(bool isSelected) {
            if (buyJokerButton != null) {
                buyJokerButton.gameObject.SetActive(isSelected && _canBuy && !_isPurchased);
            }
        }
    }
}
