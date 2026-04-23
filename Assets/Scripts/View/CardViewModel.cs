using UnityEngine;

public sealed class CardViewModel {
    public int Index = -1;
    public string RankText;
    public string SuitText;
    public Color AccentColor;
    public bool IsSelected;
    public bool IsInteractable = true;
}