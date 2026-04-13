using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, IDragHandler,IBeginDragHandler, IEndDragHandler
{
    private Vector3 _offset;
    private bool _isDragging;
    private bool _wasDragged;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image imageComponent;
    private Camera _camera;

    private void Start() {
        _camera = Camera.main;
    }

    void Update() {
        //ClampPosition();

        if (!_isDragging) return;
        if (!_camera) return;
        
        Vector2 targetPosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - _offset;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        Vector2 velocity = direction * Mathf.Min(50f, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
        transform.Translate(velocity * Time.deltaTime);
    }
    
    public void OnPointerDown(PointerEventData eventData) {
        Debug.Log("Card Clicked");
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Debug.Log("OnPointerEnter");
    }

    public void OnPointerUp(PointerEventData eventData) {
        Debug.Log("OnPointerUp");
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (Camera.main != null) {
            Debug.Log("OnBeginDrag");
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _offset = mousePosition - (Vector2)transform.position;
        }

        _isDragging = true;
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;

        _wasDragged = true;
    }

    public void OnEndDrag(PointerEventData eventData) {
        Debug.Log("OnEndDrag");
        _isDragging = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;
    }

    public void OnDrag(PointerEventData eventData) {
        
    }
}
