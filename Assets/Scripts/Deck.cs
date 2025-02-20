using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
public class Deck : MonoBehaviour
{
    [SerializeField] private List<Card> cards;
    [SerializeField] private Card selectedCard;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private bool tweenCardReturn = true;
    bool isCrossing = false;
    private RectTransform rect;
    public int cardsToSpawn = 10;
    void Start()
    {
        for (int i = 0; i < cardsToSpawn; i++)
        {
            GameObject slot = Instantiate(slotPrefab, transform);
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0, 0.5f);
            slotRect.anchorMax = new Vector2(0, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
        }
        rect = GetComponent<RectTransform>();
        cards = GetComponentsInChildren<Card>().ToList();
        int cardCount = 0;
        foreach (Card card in cards)
        {
            card.BeginDragEvent.AddListener(BeginDrag);
            card.EndDragEvent.AddListener(EndDrag);
            card.name = cardCount.ToString();
            cardCount++;
        }
        StartCoroutine(Frame());
    }
    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }
    void EndDrag(Card card)
    {
        if (selectedCard == null)
            return;
        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0, selectedCard.selectionOffset, 0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack).OnComplete(() =>
        {
            foreach (Card c in cards)
            {
                c.cardVisual.UpdateIndex(cards.Count);
            }
        });
        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;
        selectedCard = null;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {
                card.Deselect();
            }
        }
        if (selectedCard == null)
            return;
        for (int i = 0; i < cards.Count; i++)
        {
            if (selectedCard.transform.position.x > cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
            if (selectedCard.transform.position.x < cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }
    void Swap(int index)
    {
        isCrossing = true;
        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;
        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);
        isCrossing = false;
        if (cards[index].cardVisual == null)
            return;
        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);
        //Updated Visual Indexes
        foreach (Card card in cards)
        {
            card.cardVisual.UpdateIndex(transform.childCount);
        }
    }
    IEnumerator Frame()
    {
        yield return new WaitForEndOfFrame();
        foreach (Card card in cards)
        {
            card.transform.localPosition = Vector3.zero;
            card.cardVisual?.UpdateIndex(cards.Count);
        }
    }
}
