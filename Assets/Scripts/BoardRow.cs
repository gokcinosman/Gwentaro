using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class BoardRow : MonoBehaviour
{
    public int ownerPlayerId; // Bu satırın hangi oyuncuya ait olduğunu belirler
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private int rowScore;
    public CardType rowType;
    public List<Card> cards = new List<Card>();
    public int RowIndex;
    private void Awake()
    {
        // cards listesini başlat
        if (cards == null)
        {
            cards = new List<Card>();
        }
    }
    public void AddCard(Card card, int playerId)
    {
        // Detaylı hata ayıklama bilgisi
        Debug.Log($"[BoardRow] AddCard çağrıldı: card={card?.name ?? "null"}, playerId={playerId}");
        // Null kontrolü
        if (card == null)
        {
            Debug.LogError("[BoardRow] AddCard metoduna null kart gönderildi!");
            return;
        }
        // cards listesi kontrolü
        if (cards == null)
        {
            // cards listesi null ise başlat
            cards = new List<Card>();
            Debug.Log("[BoardRow] cards listesi başlatıldı.");
        }
        // Kartı listeye ekle
        cards.Add(card);
        // Kartı yerleştir
        card.GetComponent<Selectable>().enabled = false;
        card.RemoveFromDeck();
        card.transform.SetParent(transform);
        RearrangeCards();
        UpdateRowValue(GetTotalPower());
        Debug.Log($"[BoardRow] Kart başarıyla eklendi: {card.name}");
    }
    private void RearrangeCards()
    {
        if (cards.Count == 0 || this == null) return;
        float cardWidth = cards[0].GetComponent<RectTransform>().rect.width;
        float rowWidth = GetComponent<RectTransform>().rect.width;
        float startX = -((cards.Count - 1) * cardWidth) / 2f;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null) continue;
            float xPos = startX + (i * cardWidth);
            Vector3 targetPos = new Vector3(xPos, 0, -i * 0.1f);
            // Capture the card reference locally
            var card = cards[i];
            card.transform.DOLocalMove(targetPos, 0.3f)
                .SetEase(Ease.OutBack)
                .OnStart(() => card.transform.SetAsLastSibling());
        }
    }
    public int GetTotalPower()
    {
        // Null kontrolü
        if (cards == null)
        {
            Debug.LogWarning("[BoardRow] GetTotalPower: cards null!");
            return 0;
        }
        int totalPower = 0;
        // Tüm kartların gücünü topla
        foreach (Card card in cards)
        {
            if (card != null && card.cardStats != null)
            {
                totalPower += card.cardStats.cardValue;
            }
        }
        return totalPower;
    }
    #region Observer Pattern
    private List<IObserver> observers = new List<IObserver>();
    public void AddObserver(IObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
            // Yeni observer'a mevcut değeri hemen bildir
            observer.OnNotify(rowScore.ToString(), this);
        }
    }
    public void RemoveObserver(IObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }
    public void NotifyObservers(string message)
    {
        foreach (var observer in observers)
        {
            if (observer != null)
            {
                observer.OnNotify(message, this);
            }
        }
    }
    public void UpdateRowValue(int newValue)
    {
        if (rowScore != newValue)
        {
            rowScore = newValue;
            NotifyObservers(rowScore.ToString());
            scoreManager.UpdateTotalScores();
        }
    }
    #endregion
}
