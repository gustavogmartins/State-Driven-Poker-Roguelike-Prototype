using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class JokerTooltipView : MonoBehaviour {
    private static readonly Color32 BackgroundColor = new(28, 37, 42, 245);
    private static readonly Color32 TextColor = new(244, 248, 249, 255);
    private static readonly Color32 BodyColor = new(198, 210, 214, 255);

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Image accentImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Canvas canvasOverride;

    private RectTransform _rectTransform;
    public RectTransform RectTransform => ResolveRectTransform();

    private void Awake() {
        ResolveRectTransform();
        ResolveCanvasGroup();
        ResolveCanvas();
        EnsureDefaultContent();
        Hide();
    }

    public void Bind(string title, string body, Color accentColor) {
        EnsureDefaultContent();

        if (titleText != null) {
            titleText.text = string.IsNullOrWhiteSpace(title) ? "Joker" : title;
        }

        if (bodyText != null) {
            bodyText.text = string.IsNullOrWhiteSpace(body) ? string.Empty : body;
        }

        if (accentImage != null) {
            accentImage.color = accentColor;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
    }

    public void Show() {
        CanvasGroup group = ResolveCanvasGroup();
        group.alpha = 1f;
        gameObject.SetActive(true);
    }

    public void Hide() {
        CanvasGroup group = ResolveCanvasGroup();
        group.alpha = 0f;
        gameObject.SetActive(false);
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

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        return canvasGroup;
    }
    
    private Canvas ResolveCanvas() {
        if (canvasOverride == null) {
            canvasOverride = GetComponent<Canvas>();
            if (canvasOverride == null) {
                canvasOverride = gameObject.AddComponent<Canvas>();
            }
        }
        canvasOverride.overrideSorting = true;
        canvasOverride.sortingOrder = 1000;
        return canvasOverride;
    }

    private void EnsureDefaultContent() {
        Image background = GetComponent<Image>();
        if (background == null) {
            background = gameObject.AddComponent<Image>();
        }

        background.color = BackgroundColor;
        background.raycastTarget = false;

        RectTransform rectTransform = RectTransform;
        rectTransform.sizeDelta = rectTransform.sizeDelta == Vector2.zero
            ? new Vector2(280f, 136f)
            : rectTransform.sizeDelta;

        accentImage ??= FindChildComponent<Image>("Accent");
        titleText ??= FindChildComponent<TextMeshProUGUI>("Title");
        bodyText ??= FindChildComponent<TextMeshProUGUI>("Body");

        if (accentImage == null) {
            accentImage = CreateAccent();
        }

        if (titleText == null) {
            titleText = CreateText("Title", 20f, FontStyles.Bold, TextColor, new Vector2(16f, -12f), new Vector2(-32f, 28f));
        }

        if (bodyText == null) {
            bodyText = CreateText("Body", 30f, FontStyles.Normal, BodyColor, new Vector2(16f, -46f), new Vector2(-32f, 72f));
        }
    }

    private Image CreateAccent() {
        var accentObject = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        accentObject.transform.SetParent(transform, false);

        RectTransform accentTransform = (RectTransform)accentObject.transform;
        accentTransform.anchorMin = new Vector2(0f, 0f);
        accentTransform.anchorMax = new Vector2(0f, 1f);
        accentTransform.pivot = new Vector2(0f, 0.5f);
        accentTransform.anchoredPosition = Vector2.zero;
        accentTransform.sizeDelta = new Vector2(6f, 0f);

        Image image = accentObject.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private TextMeshProUGUI CreateText(
        string objectName,
        float fontSize,
        FontStyles fontStyle,
        Color color,
        Vector2 anchoredPosition,
        Vector2 sizeDelta) {
        var textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(transform, false);

        RectTransform textTransform = (RectTransform)textObject.transform;
        textTransform.anchorMin = new Vector2(0f, 1f);
        textTransform.anchorMax = new Vector2(1f, 1f);
        textTransform.pivot = new Vector2(0f, 1f);
        textTransform.anchoredPosition = anchoredPosition;
        textTransform.sizeDelta = sizeDelta;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        return text;
    }

    private T FindChildComponent<T>(string childName) where T : Component {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<T>() : null;
    }
}
