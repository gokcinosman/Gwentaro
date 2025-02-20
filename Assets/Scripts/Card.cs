using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
public class Card : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    private RectTransform rectTransform;
    public CardVisual cardVisual;
    private Canvas canvas;
    private bool isDragging = false;
    private Vector2 offset;
    public UnityEvent<Card> BeginDragEvent;
    public UnityEvent<Card> EndDragEvent;
    public float selectionOffset = 50;
    public bool selected;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        isDragging = true;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        // Tıklama olayını yakalıyoruz
    }
    public void OnDrag(PointerEventData eventData)
    {
    }
    private void Update()
    {
        if (isDragging)
        {
            // Mouse pozisyonunu UI koordinatlarına çeviriyoruz
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            Input.mousePosition,
            canvas.worldCamera,
            out position);
            // Objenin pozisyonunu güncelliyoruz
            rectTransform.position = canvas.transform.TransformPoint(position);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        isDragging = false;
        rectTransform.localPosition = Vector3.zero;
    }
    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            // if (selected)
            //     transform.localPosition += (cardVisual.transform.up * 50);
            // else
            //     transform.localPosition = Vector3.zero;
        }
    }
    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }
}