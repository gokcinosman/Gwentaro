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
    [SerializeField] private RemaningCards remaningCardsDeck;
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
            // Kart seçimini hemen gerçekleştir, gecikme olmadan
            SelectRandomCardsForGame();
        }
        else
        {
            StartCoroutine(WaitForCards());
            // Diğer oyuncunun destesi için de RemainingCards'ı göster
            if (remaningCardsDeck != null)
            {
                InitializeRemainingCardsForRemotePlayer();
            }
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
        Card card = cardView.GetComponentInChildren<Card>();
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
            // Burada card.isPlaced'ı kontrol et, zaten yerleştirilmişse işlemi atla
            if (card.isPlaced)
            {
                Debug.Log($"[Deck] {card.name} kartı zaten yerleştirilmiş, desteden çıkarma işlemi atlanıyor.");
                // Listeden çıkar ama düzenleme yapma
                cards.Remove(card);
                return;
            }
            // Kart nesnesini takip et
            Debug.Log($"[Deck] {card.name} kartı desteden çıkarılıyor. Kart ID: {card.GetInstanceID()}");
            // Kartı listeden çıkar
            cards.Remove(card);
            // Kartların yerlerini yeniden düzenle
            RearrangeRemainingCards();
        }
    }
    private void RearrangeRemainingCards()
    {
        if (cards.Count == 0) return;
        // Aktif slot'ları ve kartları bul
        List<Transform> activeSlots = new List<Transform>();
        List<Card> activeCards = new List<Card>();
        // Aktif kartları ve slotları topla
        foreach (Card card in cards)
        {
            if (card != null && !card.isPlaced && !card.isDiscarded)
            {
                activeCards.Add(card);
            }
        }
        // Aktif slotları bul
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform slot = transform.GetChild(i);
            if (slot.gameObject.activeSelf)
            {
                activeSlots.Add(slot);
            }
        }
        Debug.Log($"[Deck] RearrangeRemainingCards: Aktif kart sayısı: {activeCards.Count}, Aktif slot sayısı: {activeSlots.Count}");
        // Aktif kartları aktif slotlara yerleştir
        int slotIndex = 0;
        foreach (Card card in activeCards)
        {
            if (slotIndex < activeSlots.Count)
            {
                // Kartı slota yerleştir
                Transform slot = activeSlots[slotIndex];
                card.transform.SetParent(slot);
                card.transform.DOLocalMove(Vector3.zero, 0.3f)
                    .SetEase(Ease.OutBack);
                slotIndex++;
            }
        }
        // Kullanılmayan slot'ları devre dışı bırak (kapatma yerine son sıraya al)
        for (int i = activeCards.Count; i < transform.childCount; i++)
        {
            if (i < transform.childCount)
            {
                Transform slot = transform.GetChild(i);
                slot.SetAsLastSibling();
            }
        }
        // Tüm aktif kartların görsel indexlerini güncelle
        foreach (Card card in activeCards)
        {
            if (card.cardVisual != null)
            {
                card.cardVisual.UpdateIndex(activeCards.Count);
            }
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
    public int DrawCards(int cardCount)
    {
        return 0;
    }
    public void DrawInitialHand()
    {
        // Artık Start'ta çağrıldığı için tekrar çağrılmasına gerek yok
        Debug.Log("[Deck] DrawInitialHand çağrıldı, ancak kartlar zaten seçilmiş durumda.");
    }
    // Diğer oyuncunun RemainingCards'ını başlat
    private void InitializeRemainingCardsForRemotePlayer()
    {
        // Uzak oyuncu için varsayılan değerlerle başlat, daha sonra PhotonView ile senkronize edilecek
        StartCoroutine(WaitForRemainingCards());
    }
    // Uzak oyuncunun kalan kart sayısını senkronize etmek için bekleme
    private IEnumerator WaitForRemainingCards()
    {
        yield return new WaitForSeconds(1.5f);
        // PhotonView ile kart sayıları diğer oyuncuya aktarılmış olmalı
        // Diğer oyuncunun kalan kart sayısını RPC ile iste
        photonView.RPC("RequestRemainingCardCount", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber);
    }
    [PunRPC]
    void RequestRemainingCardCount(int requestingPlayerActorNumber)
    {
        // Kendi kalan kart sayımızı RPC ile diğer oyuncuya bildir
        int remainingCount = 0;
        if (cards != null)
        {
            // Aktif kartları hariç tut, sadece dokunulmamış deste kartlarını say
            remainingCount = FindInactiveCardCount();
        }
        // İsteyen oyuncuya kalan kart sayısını bildir
        photonView.RPC("UpdateRemainingCardCount", RpcTarget.All, ownerPlayerId, remainingCount);
    }
    [PunRPC]
    void UpdateRemainingCardCount(int playerIndex, int cardCount)
    {
        if (remaningCardsDeck != null)
        {
            // Sadece ilgili oyuncunun RemaningCards nesnesini güncelle
            if (playerIndex == ownerPlayerId)
            {
                remaningCardsDeck.SetRemainingCardsCount(cardCount);
                Debug.Log($"[Deck] Player{playerIndex}'ın kalan kart sayısı güncellendi: {cardCount}");
            }
        }
    }
    // Aktif olmayan kart sayısını bul
    private int FindInactiveCardCount()
    {
        int inactiveCardCount = 0;
        // Transform hiyerarşisinde aktif olmayan slot ve kart sayısını say
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform slot = transform.GetChild(i);
            if (!slot.gameObject.activeSelf)
            {
                inactiveCardCount++;
            }
        }
        return inactiveCardCount;
    }
    public void SelectRandomCardsForGame()
    {
        if (cards == null || cards.Count == 0)
            return;
        // Tüm desteden rastgele 10 kart seç
        // Seçilen kartlar elimizde kalacak, diğerleri dokunulmamış destede kalacak
        if (cards.Count <= 10)
        {
            Debug.LogWarning("[Deck] Destede zaten 10 veya daha az kart var, tüm kartlar kullanılacak.");
            return;
        }
        // Kartları karıştır
        List<Card> shuffledCards = new List<Card>(cards);
        for (int i = 0; i < shuffledCards.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, shuffledCards.Count);
            Card temp = shuffledCards[i];
            shuffledCards[i] = shuffledCards[randomIndex];
            shuffledCards[randomIndex] = temp;
        }
        // İlk 10 kartı elimize alacağız, geri kalanı dokunulmamış destede kalacak
        List<Card> handCards = shuffledCards.GetRange(0, 10);
        List<Card> remainingCards = shuffledCards.GetRange(10, shuffledCards.Count - 10);
        // Kalan kartları dokunulmamış desteye taşı
        foreach (Card remainingCard in remainingCards)
        {
            if (remainingCard != null && remainingCard.gameObject != null)
            {
                // Kart slotunu bul ve devre dışı bırak
                remainingCard.gameObject.SetActive(false);
                Transform cardSlot = remainingCard.transform.parent;
                if (cardSlot != null)
                {
                    cardSlot.gameObject.SetActive(false);
                }
            }
        }
        // Aktif kart listesini güncelle (sadece el kartları)
        cards.Clear();
        cards.AddRange(handCards);
        // Kalan kart sayısını RemainingCards nesnesine bildir ve RPC ile diğer oyunculara da gönder
        int remainingCount = remainingCards.Count;
        if (remaningCardsDeck != null)
        {
            remaningCardsDeck.SetRemainingCardsCount(remainingCount);
            // Kalan kart sayısını tüm oyunculara bildir
            photonView.RPC("UpdateRemainingCardCount", RpcTarget.Others, ownerPlayerId, remainingCount);
        }
        else
        {
            Debug.LogError("[Deck] remaningCardsDeck referansı atanmamış!");
        }
        // Önce tüm aktif slotları bulalım
        List<Transform> activeSlots = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform slot = transform.GetChild(i);
            if (slot.gameObject.activeSelf)
            {
                activeSlots.Add(slot);
            }
        }
        // El kartlarını sırayla aktif slotlara yerleştirelim
        for (int i = 0; i < handCards.Count; i++)
        {
            if (handCards[i] != null && handCards[i].gameObject != null)
            {
                handCards[i].gameObject.SetActive(true);
                // Kartı uygun slota taşı
                if (i < activeSlots.Count)
                {
                    handCards[i].transform.SetParent(activeSlots[i]);
                    handCards[i].transform.localPosition = Vector3.zero;
                    activeSlots[i].gameObject.SetActive(true);
                }
                else if (i < transform.childCount)
                {
                    // Yeterli aktif slot yoksa, devre dışı olan slotları aktif et
                    Transform slot = transform.GetChild(i);
                    slot.gameObject.SetActive(true);
                    handCards[i].transform.SetParent(slot);
                    handCards[i].transform.localPosition = Vector3.zero;
                }
            }
        }
        Debug.Log($"[Deck] Oyun için {cards.Count} kart seçildi, {remainingCards.Count} kart dokunulmamış destede kaldı.");
        // Görsel indeksleri güncelle
        foreach (Card card in cards)
        {
            if (card.cardVisual != null)
            {
                card.cardVisual.UpdateIndex(cards.Count);
            }
        }
    }
    // Eli discard pile'a gönder (her el bitiminde çağrılabilir)
    public void DiscardHand(DiscardPile discardPile)
    {
        if (discardPile == null)
        {
            discardPile = FindObjectOfType<DiscardPile>();
            if (discardPile == null)
            {
                Debug.LogError("[Deck] Iskarta yığını bulunamadı!");
                return;
            }
        }
        // Elimizdeki tüm kartları ıskarta yığınına gönder
        List<Card> cardsToDiscard = new List<Card>(cards);
        foreach (Card card in cardsToDiscard)
        {
            // Kartı ıskarta yığınına ekle
            discardPile.AddCard(card);
            // Kartı elden çıkar
            cards.Remove(card);
            // Kart nesnesini scene'den gizle - görsel olarak devreden çıkar
            if (card.gameObject != null)
            {
                if (card.transform.parent != null)
                {
                    // Eğer kart bir slot içindeyse, slot'u da devre dışı bırak
                    Transform cardSlot = card.transform.parent;
                    if (cardSlot.gameObject != null)
                    {
                        cardSlot.gameObject.SetActive(false);
                    }
                }
                card.gameObject.SetActive(false);
            }
        }
        Debug.Log($"[Deck] {cardsToDiscard.Count} kart ıskarta yığınına gönderildi.");
    }
}
