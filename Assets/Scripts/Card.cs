using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
using Photon.Pun;
public class Card : MonoBehaviourPun, IPointerDownHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
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
    private BoardRow currentHoveredRow;
    private Deck deck;
    public bool isPlaced = false;
    public CardStats cardStats;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        deck = FindObjectOfType<Deck>();
        if (deck == null)
        {
            Debug.LogError("[Card] Deck bileşeni sahnede bulunamadı! Deck'in sahnede aktif olduğundan emin olun.");
        }
    }
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[Card] Canvas bulunamadı");
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced || isDragging)
            return;
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
        if (isPlaced)
            return;
        if (isPlaced || canvas == null) // Canvas'ın null olup olmadığını kontrol et
        {
            Debug.LogError("[Card] OnDrag sırasında Canvas null!");
            return;
        }
        // Mouse pozisyonunu dünya koordinatlarına çeviriyoruz
        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            Input.mousePosition,
            canvas.worldCamera,
            out mousePosition);
        mousePosition = canvas.transform.TransformPoint(mousePosition);
        // Raycast için contact filter ayarları
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(LayerMask.GetMask("BoardRow"));
        // Raycast için hit array
        RaycastHit2D[] hits = new RaycastHit2D[10];
        int hitCount = Physics2D.Raycast(mousePosition, Vector2.zero, filter, hits);
        BoardRow hoveredRow = null;
        float closestDistance = float.MaxValue;
        // En yakın BoardRow'u buluyoruz
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = hits[i];
            BoardRow row = hit.transform.GetComponent<BoardRow>();
            if (row != null)
            {
                float distance = Vector2.Distance(mousePosition, hit.point);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    hoveredRow = row;
                }
            }
        }
        // Hover durumunu güncelliyoruz
        if (hoveredRow != currentHoveredRow)
        {
            currentHoveredRow = hoveredRow;
        }
        // Kartın pozisyonunu güncelle
        if (isDragging)
        {
            rectTransform.position = mousePosition;
        }
    }
    private void Update()
    {
        if (isDragging && !isPlaced)
        {
            if (canvas == null || rectTransform == null)
            {
                Debug.LogError("[Card] Update sırasında canvas veya rectTransform null!");
                return;
            }
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                Input.mousePosition,
                canvas.worldCamera,
                out position);
            rectTransform.position = canvas.transform.TransformPoint(position);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced)
            return;
        isDragging = false;
        EndDragEvent.Invoke(this);
        if (currentHoveredRow != null)
        {
            int currentPlayerId = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? 0 : 1;
            int rowIndex = currentHoveredRow.RowIndex; // BoardRow içindeki indeks
            if (rowIndex < 0)
            {
                Debug.LogError($"[OnEndDrag] HATALI rowIndex: {rowIndex}. BoardRow yanlış ayarlanmış olabilir.");
                ResetPosition(); // Kartı geri eski yerine götür
                return;
            }
            GameManager.Instance.PlayCard(currentPlayerId, this, currentHoveredRow);
            currentHoveredRow = null;
        }
        else
        {
            Debug.LogError("[OnEndDrag] currentHoveredRow null! Kart yanlış yere bırakılıyor olabilir.");
            ResetPosition();
        }
    }
    public void Deselect()
    {
        if (selected)
        {
            selected = false;
        }
    }
    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }
    public void RemoveFromDeck()
    {
        deck.RemoveCard(this);
    }
    public void ResetPosition()
    {
        if (!isPlaced)
        {
            isDragging = false;
            isPlaced = false;
            // Mevcut parent'ın merkezine dönüş
            Vector3 targetPosition = selected ?
                new Vector3(0, selectionOffset, 0) :
                Vector3.zero;
            // Animasyonu sadece gerekliyse çalıştır
            if (transform.localPosition != targetPosition)
            {
                transform.DOLocalMove(targetPosition, 0.2f)
                    .SetEase(Ease.OutQuad);
            }
        }
    }
}