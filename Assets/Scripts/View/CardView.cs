using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerUpHandler {
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private Image cardImage;
    
    private CardViewModel _viewModel;
    public event Action<int> OnCardSelected;
    
    public void Bind(CardViewModel viewModel) {
        _viewModel = viewModel;
        
        cardName.text = viewModel.CardName;
        cardImage.color = viewModel.IsSelected ? Color.softYellow : Color.white;
    }

    public void OnPointerUp(PointerEventData eventData) {
        OnCardSelected?.Invoke(_viewModel.Index);
    }
}