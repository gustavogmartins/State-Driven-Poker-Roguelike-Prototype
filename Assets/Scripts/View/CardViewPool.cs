using System.Collections.Generic;
using UnityEngine;

public sealed class CardViewPool : MonoBehaviour {
    [SerializeField] private CardView cardPrefab;

    private readonly Stack<CardView> _pool = new();

    public void Configure(CardView prefab) {
        if (prefab != null) {
            cardPrefab = prefab;
        }
    }

    public CardView Get(RectTransform parent) {
        if (cardPrefab == null || parent == null) {
            return null;
        }

        CardView view = _pool.Count > 0
            ? _pool.Pop()
            : Instantiate(cardPrefab);

        view.transform.SetParent(parent, false);
        view.gameObject.SetActive(true);
        view.ResetView();
        return view;
    }

    public void Release(CardView view) {
        if (view == null) {
            return;
        }

        view.ResetView();
        view.gameObject.SetActive(false);
        view.transform.SetParent(transform, false);
        _pool.Push(view);
    }
}
