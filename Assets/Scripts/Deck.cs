using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;
using Photon.Pun;
public class Deck : MonoBehaviour
{
    [NonSerialized] private List<Card> cards;
    [SerializeField] private Card selectedCard;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private bool tweenCardReturn = true;
    [SerializeField] private PhotonView photonView;
    bool isCrossing = false;
    private RectTransform rect;
    public int cardsToSpawn = 10;
    public int ownerPlayerId;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GenerateDeck(ownerPlayerId);
        }
        else
        {
            StartCoroutine(WaitForCards());
        }
        SetDeckVisibility();
        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        // Sadece kendi destesi için Frame() çalıştır
        if (ownerPlayerId == localPlayerId)
        {
            StartCoroutine(Frame());
        }
    }
    void SetDeckVisibility()
    {
        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (ownerPlayerId != localPlayerId)
        {
            GameObject enemyDeck = GameObject.Find($"Deck_Player{ownerPlayerId}");
            if (enemyDeck != null)
            {
                enemyDeck.SetActive(false);
            }
        }
    }
    public void GenerateDeck(int playerId)
    {
        cards = new List<Card>();
        for (int i = 0; i < cardsToSpawn; i++)
        {
            GameObject newCardSlot = PhotonNetwork.Instantiate("CardSlot", transform.position, Quaternion.identity);
            RectTransform slotRect = newCardSlot.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0, 0.5f);
            slotRect.anchorMax = new Vector2(0, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            PhotonView slotPhotonView = newCardSlot.GetComponent<PhotonView>();
            if (slotPhotonView != null)
            {
                photonView.RPC("SetCardSlotParent", RpcTarget.AllBuffered, slotPhotonView.ViewID);
            }
            else
            {
                Debug.LogError("[Deck] CardSlot için PhotonView bulunamadı! Index: " + i);
            }
            Card cardComponent = newCardSlot.GetComponentInChildren<Card>();
            if (cardComponent != null)
            {
                cards.Add(cardComponent);
                cardComponent.BeginDragEvent.AddListener(BeginDrag);
                cardComponent.EndDragEvent.AddListener(EndDrag);
                cardComponent.name = $"Player{playerId}_Card{i}";
            }
            else
            {
                Debug.LogError("[Deck] CardSlot içinde Card bileşeni bulunamadı! Index: " + i);
            }
        }
    }
    // Kaydedilmiş deste bilgilerinden kart oluştur
    public void GenerateDeckFromCards(int playerId, List<CardStats> deckCards, CardStats leaderCard = null)
    {
        cards = new List<Card>();
        // Önce normal deste kartlarını oluştur
        for (int i = 0; i < deckCards.Count; i++)
        {
            GameObject newCardSlot = PhotonNetwork.Instantiate("CardSlot", transform.position, Quaternion.identity);
            RectTransform slotRect = newCardSlot.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0, 0.5f);
            slotRect.anchorMax = new Vector2(0, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            PhotonView slotPhotonView = newCardSlot.GetComponent<PhotonView>();
            if (slotPhotonView != null)
            {
                photonView.RPC("SetCardSlotParent", RpcTarget.AllBuffered, slotPhotonView.ViewID);
            }
            else
            {
                Debug.LogError("[Deck] CardSlot için PhotonView bulunamadı! Index: " + i);
            }
            Card cardComponent = newCardSlot.GetComponentInChildren<Card>();
            if (cardComponent != null)
            {
                // Kart bilgilerini ayarla
                cardComponent.cardStats = deckCards[i];
                // Kart görselini güncelle
                if (cardComponent.cardVisual != null)
                {
                    cardComponent.cardVisual.UpdateVisual(deckCards[i]);
                }
                cards.Add(cardComponent);
                cardComponent.BeginDragEvent.AddListener(BeginDrag);
                cardComponent.EndDragEvent.AddListener(EndDrag);
                cardComponent.name = $"Player{playerId}_Card{i}";
            }
            else
            {
                Debug.LogError("[Deck] CardSlot içinde Card bileşeni bulunamadı! Index: " + i);
            }
        }
        // Eğer lider kartı varsa, onu da oluştur (oyun mekanizmasına bağlı olarak)
        if (leaderCard != null)
        {
            // Lider kartı için özel işlemler burada yapılabilir
            // Örneğin: CreateLeaderCard(leaderCard);
        }
    }
    [PunRPC]
    void SetCardSlotParent(int slotViewID)
    {
        PhotonView slotPhotonView = PhotonView.Find(slotViewID);
        if (slotPhotonView != null)
        {
            slotPhotonView.transform.SetParent(transform, false);
        }
        else
        {
            Debug.LogError("[Deck] SetCardSlotParent başarısız! ViewID: " + slotViewID);
        }
    }
    IEnumerator WaitForCards()
    {
        yield return new WaitForSeconds(1f);
        cards = FindObjectsOfType<Card>().ToList();
        foreach (Card card in cards)
        {
            card.BeginDragEvent.AddListener(BeginDrag);
            card.EndDragEvent.AddListener(EndDrag);
        }
    }
    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }
    void EndDrag(Card card)
    {
        if (selectedCard == null || selectedCard.isPlaced)
            return;
        if (!selectedCard.isPlaced)
        {
            selectedCard.transform.DOLocalMove(selectedCard.selected ?
                new Vector3(0, selectedCard.selectionOffset, 0) :
                Vector3.zero,
                tweenCardReturn ? .15f : 0)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    foreach (Card c in cards.Where(c => c != null && !c.isPlaced))
                    {
                        c.cardVisual?.UpdateIndex(cards.Count);
                    }
                });
        }
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
    public void RemoveCard(Card card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            RearrangeRemainingCards();
        }
    }
    private void RearrangeRemainingCards()
    {
        if (cards.Count == 0) return;
        float cardWidth = cards[0].GetComponent<RectTransform>().rect.width;
        float startX = -((cards.Count - 1) * cardWidth) / 2f;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null || cards[i].isPlaced) continue;
            Transform targetSlot = transform.GetChild(i);
            cards[i].transform.SetParent(targetSlot);
            cards[i].transform.DOLocalMove(Vector3.zero, 0.3f)
                .SetEase(Ease.OutBack);
        }
        // Kullanılmayan slot'ları sağa kaydır
        for (int i = cards.Count; i < transform.childCount; i++)
        {
            Transform slot = transform.GetChild(i);
            slot.SetAsLastSibling();
        }
    }
}
