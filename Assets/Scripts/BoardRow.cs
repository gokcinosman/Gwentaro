using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class BoardRow : MonoBehaviour
{
    public int ownerPlayerId; // Bu satırın hangi oyuncuya ait olduğunu belirler

    public CardType rowType;
    public List<Card> cards = new List<Card>();
    public int RowIndex;

    public bool AddCard(Card card, int playerId)
    {
        if (GameManager.Instance.CurrentTurnPlayer != playerId)
        {
            Debug.LogWarning($"[AddCard] Oyuncu {playerId} sırası değilken kart oynayamaz!");
            return false;
        }

        if (playerId != ownerPlayerId)
        {
            Debug.LogError($"[AddCard] HATA: Oyuncu {playerId}, kendisine ait olmayan bir satıra kart eklemeye çalıştı!");
            return false;
        }

        if (cards.Contains(card))
        {
            Debug.LogError("[AddCard] HATA: Kart zaten bu satırda mevcut!");
            return false;
        }

        if (rowType != card.cardStats.cardType)
        {
            Debug.LogError("[AddCard] HATA: Yanlış türde bir kart eklenmeye çalışılıyor!");
            return false;
        }

        // Kartı ekle
        cards.Add(card);
        card.GetComponent<Selectable>().enabled = false;
        card.RemoveFromDeck();
        card.transform.SetParent(transform);

        RearrangeCards();
        Debug.Log($"[AddCard] Oyuncu {playerId} kartı başarıyla ekledi.");
        return true;
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
        return 0;
    }
}
