using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScorePopupView : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI chipText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Color32 textColor; 
    private RectTransform _rectTransform;

    public RectTransform RectTransform => ResolveRectTransform();
    public CanvasGroup CanvasGroup => ResolveCanvasGroup();

    private void Awake() {
        ResolveRectTransform();
        ResolveCanvasGroup();
        ResolveText();
        DisableRaycastTargets();
    }

    public void Bind(int chipValue) {
        Bind($"+{chipValue}");
    }

    public void Bind(string text) {
        ResolveText();

        if (chipText != null) {
            chipText.text = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
            chipText.color = textColor;
        }
    }

    public void ResetView() {
        RectTransform rectTransform = RectTransform;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;

        CanvasGroup.alpha = 0f;
        DisableRaycastTargets();
    }

    private RectTransform ResolveRectTransform() {
        if (_rectTransform == null) {
            _rectTransform = (RectTransform)transform;
        }

        return _rectTransform;
    }

    private CanvasGroup ResolveCanvasGroup() {
        if (canvasGroup == null) {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        return canvasGroup;
    }

    private void ResolveText() {
        if (chipText == null) {
            chipText = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        }
    }

    private void DisableRaycastTargets() {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(includeInactive: true);
        for (int i = 0; i < graphics.Length; i++) {
            graphics[i].raycastTarget = false;
        }
    }
}
