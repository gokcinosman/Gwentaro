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
        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (ownerPlayerId == localPlayerId)
        {
            LoadSavedDeck(localPlayerId);
            StartCoroutine(Frame());
        }
        else
        {
            StartCoroutine(WaitForCards());
        }
        SetDeckVisibility();
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
    public void GenerateDeckFromCards(int playerId, List<CardStats> deckCards)
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
                // Kart verilerini senkronize et
                photonView.RPC("SyncCardStats", RpcTarget.AllBuffered, slotPhotonView.ViewID, deckCards[i].name);
            }
            else
            {
                Debug.LogError("[Deck] CardSlot için PhotonView bulunamadı! Index: " + i);
            }
            Card cardComponent = newCardSlot.GetComponentInChildren<Card>();
            if (cardComponent != null)
            {
                // Kart bilgilerini ayarla (yerel olarak)
                cardComponent.cardStats = deckCards[i];
                // Kart görselini güncelle
                CardStatsVisual cardVisual = cardComponent.GetComponent<CardStatsVisual>();
                if (cardVisual != null)
                {
                    cardVisual.UpdateVisual(deckCards[i]);
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
    [PunRPC]
    void SyncCardStats(int cardViewID, string cardStatsName)
    {
        Debug.Log($"[Deck] SyncCardStats çağrıldı: cardViewID={cardViewID}, cardStatsName={cardStatsName}");
        // Kart nesnesini bul
        PhotonView cardView = PhotonView.Find(cardViewID);
        if (cardView == null)
        {
            Debug.LogError($"[Deck] cardViewID={cardViewID} için PhotonView bulunamadı!");
            return;
        }
        Card card = cardView.GetComponent<Card>();
        if (card == null)
        {
            Debug.LogError($"[Deck] cardViewID={cardViewID} için Card bileşeni bulunamadı!");
            return;
        }
        // CardStats'ı yükle
        CardStats cardStats = Resources.Load<CardStats>($"Cards/{cardStatsName}");
        if (cardStats == null)
        {
            Debug.LogError($"[Deck] CardStats bulunamadı: {cardStatsName}");
            return;
        }
        // Kart verilerini ayarla
        card.cardStats = cardStats;
        // Kart görselini güncelle
        CardStatsVisual cardVisual = card.GetComponent<CardStatsVisual>();
        if (cardVisual != null)
        {
            cardVisual.UpdateVisual(cardStats);
        }
        Debug.Log($"[Deck] Kart senkronize edildi: {cardStatsName}");
    }
    IEnumerator WaitForCards()
    {
        yield return new WaitForSeconds(1f);
        cards = FindObjectsOfType<Card>().Where(c => c != null).ToList();
        foreach (Card card in cards)
        {
            if (card != null)
            {
                card.BeginDragEvent.AddListener(BeginDrag);
                card.EndDragEvent.AddListener(EndDrag);
            }
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
        if (cards != null && cards.Count > 0)
        {
            foreach (Card card in cards.Where(c => c != null))
            {
                card.transform.localPosition = Vector3.zero;
                if (card.cardVisual != null)
                {
                    card.cardVisual.UpdateIndex(cards.Count);
                }
            }
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
    // Kaydedilmiş desteyi yükle
    private void LoadSavedDeck(int playerId)
    {
        // PlayerPrefs'ten desteyi al
        if (PlayerPrefs.HasKey("PlayerDeck"))
        {
            string deckJson = PlayerPrefs.GetString("PlayerDeck");
            DeckData deckData = JsonUtility.FromJson<DeckData>(deckJson);
            // Kart istatistiklerini yükle
            List<CardStats> deckCards = new List<CardStats>();
            // Tüm kartları Resources klasöründen yükle
            CardStats[] allCards = Resources.LoadAll<CardStats>("Cards");
            // Deste kartlarını bul
            foreach (string cardName in deckData.cardNames)
            {
                CardStats cardStats = System.Array.Find(allCards, c => c.name == cardName);
                if (cardStats != null)
                {
                    deckCards.Add(cardStats);
                }
                else
                {
                    Debug.LogWarning($"[Deck] Kaydedilmiş destede bulunan kart bulunamadı: {cardName}");
                }
            }
            GenerateDeckFromCards(playerId, deckCards);
            Debug.Log($"[Deck] Kaydedilmiş deste yüklendi: {deckCards.Count} kart");
        }
        else
        {
            // Kaydedilmiş deste yoksa varsayılan desteyi oluştur
            Debug.Log("[Deck] Kaydedilmiş deste bulunamadı. Varsayılan deste oluşturuluyor.");
            GenerateDeck(playerId);
        }
    }
    // DeckData sınıfını ekle (BuildDeck ile aynı yapıda olmalı)
    [System.Serializable]
    private class DeckData
    {
        public string[] cardNames;
    }
}
