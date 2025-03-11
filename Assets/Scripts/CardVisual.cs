using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CardVisual : MonoBehaviour
{
    private Transform _transform;
    private Card parentCard;
    private RectTransform _rectTransform;
    private Vector3 _targetPosition;
    private bool isMoving;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private float xOffset = 200f;
    // Kart görsel elemanları
    [SerializeField] private Image cardImage;
    [SerializeField] private Image cardFrame;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardValueText;
    [SerializeField] private TextMeshProUGUI cardDescText;
    [SerializeField] private Image cardTypeIcon;
    private void Awake()
    {
        _transform = transform;
        _rectTransform = GetComponent<RectTransform>();
    }
    private void Start()
    {
        parentCard = GetComponentInParent<Card>();
        parentCard.BeginDragEvent.AddListener(BeginDrag);
        parentCard.EndDragEvent.AddListener(EndDrag);
    }
    private void BeginDrag(Card card)
    {
        isMoving = true;
    }
    private void EndDrag(Card card)
    {
        isMoving = false;
    }
    public void Swap(int direction) // -1: sola, 1: sağa
    {
        // Yeni hedef pozisyonu hesapla
        _targetPosition = new Vector3(
            direction * xOffset,
            _rectTransform.anchoredPosition.y,
            _transform.localPosition.z
        );
        isMoving = true;
    }
    private void Update()
    {
    }
    public void UpdateIndex(int length)
    {
        transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    }
    public int ParentIndex() => _transform.GetSiblingIndex();
    // Kart görselini güncelleme metodu
    public void UpdateVisual(CardStats cardStats)
    {
        if (cardStats == null)
            return;
        // Kart görselini güncelle
        if (cardImage != null && cardStats.cardSprite != null)
        {
            cardImage.sprite = cardStats.cardSprite;
        }
        // Kart adını güncelle
        if (cardNameText != null)
        {
            cardNameText.text = cardStats.cardName;
        }
        // Kart değerini güncelle
        if (cardValueText != null)
        {
            cardValueText.text = cardStats.cardValue.ToString();
            // Hero kartları için değer gösterme
            cardValueText.gameObject.SetActive(cardStats.cardStatue != CardStatus.Hero && cardStats.cardStatue != CardStatus.Leader);
        }
        // Kart açıklamasını güncelle
        if (cardDescText != null)
        {
            cardDescText.text = cardStats.cardDesc;
        }
        // Kart tipi ikonunu güncelle
        if (cardTypeIcon != null)
        {
            switch (cardStats.cardType)
            {
                case CardType.Melee:
                    cardTypeIcon.sprite = cardStats.meleeSpriteIcon;
                    break;
                case CardType.Ranged:
                    cardTypeIcon.sprite = cardStats.rangedSpriteIcon;
                    break;
                case CardType.Siege:
                    cardTypeIcon.sprite = cardStats.siegeSpriteIcon;
                    break;
                default:
                    cardTypeIcon.gameObject.SetActive(false);
                    break;
            }
            // Özel kart tipleri için ek ikonlar
            if (cardStats.cardClass == CardClass.Agile)
            {
                cardTypeIcon.sprite = cardStats.agileSpriteIcon;
            }
        }
        if (cardFrame != null)
        {
            // Burada farklı kart tipleri için farklı çerçeveler kullanabilirsiniz
            // Örnek: cardFrame.sprite = cardStats.cardStatue == CardStatus.Hero ? heroFrameSprite : normalFrameSprite;
        }
    }
}
