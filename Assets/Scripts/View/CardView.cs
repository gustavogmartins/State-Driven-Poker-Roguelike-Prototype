using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI cardName;

    public void Bind(CardViewModel viewModel) {
        cardName.text = viewModel.CardName;
    }
}