using UnityEngine;

public sealed class CardViewModel {
    public int CardId = -1;
    public CardZone Zone = CardZone.None;
    public int Index = -1;
    public string RankText;
    public string SuitText;
    public Color AccentColor;
    public bool IsSelected;
    public bool IsInteractable = true;
    public bool IsDebuffed;
    public bool CanSell;
    public bool IsSellSelected;
    public string SellButtonText;
}
