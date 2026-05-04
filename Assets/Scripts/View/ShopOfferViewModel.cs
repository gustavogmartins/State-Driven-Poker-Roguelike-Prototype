using UnityEngine;

namespace View {
    public sealed class ShopOfferViewModel {
        public int Index;
        public string TitleText;
        public string RarityText;
        public string DescriptionText;
        public string CostText;
        public string StatusText;
        public bool IsSelected;
        public bool IsPurchased;
        public bool CanBuy;
        public Color AccentColor;
        public Color RarityColor;
    }
}
