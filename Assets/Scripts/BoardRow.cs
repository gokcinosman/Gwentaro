using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class BoardRow : MonoBehaviour
{
    public CardType rowType;
    public List<Card> cards = new List<Card>();
    public bool AddCard(Card card)
    {
        if (!cards.Contains(card) && rowType == card.cardStats.cardType)
        {
            cards.Add(card);
            card.GetComponent<Selectable>().enabled = false;
            card.RemoveFromDeck();
            card.transform.SetParent(transform);
            RearrangeCards();
            return true;
        }
        return false;
    }
    private void RearrangeCards()
    {
        if (cards.Count == 0 || this == null) return;
        float cardWidth = cards[0].GetComponent<RectTransform>().rect.width;
        float rowWidth = GetComponent<RectTransform>().rect.width;
        // Merkezden başlayarak sağa doğru ekleme
        float startX = -((cards.Count - 1) * cardWidth) / 2f;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null) continue;
            float xPos = startX + (i * cardWidth);
            Vector3 targetPos = new Vector3(
                xPos,
                0,
                -i * 0.1f
            );
            cards[i].transform.DOLocalMove(targetPos, 0.3f)
                .SetEase(Ease.OutBack)
                .OnStart(() => cards[i].transform.SetAsLastSibling());
        }
    }
}
