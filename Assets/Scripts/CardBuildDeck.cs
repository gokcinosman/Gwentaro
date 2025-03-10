using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class CardBuildDeck : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public CardStats cardStats; // Doğrudan CardStats referansı
    private BuildDeck buildDeck;
    private float doubleClickTime = 0.3f; // Çift tıklama için zaman aralığı
    private float lastClickTime = 0f;
    private bool isLeaderCard = false; // Lider kartı mı?
    private bool isInPlayerDeck = false; // Kart oyuncu destesinde mi?
    private void Awake()
    {
        // BuildDeck referansını bul
        buildDeck = FindObjectOfType<BuildDeck>();
        if (buildDeck == null)
        {
            Debug.LogError("[CardBuildDeck] BuildDeck referansı bulunamadı!");
            return; // BuildDeck yoksa devam etme
        }
        // Eğer cardStats null ise ve buildDeck varsa, kart adına göre uygun CardStats'ı bul
        if (cardStats == null)
        {
            // Kart adına göre BuildDeck'ten uygun CardStats'ı bul
            string cardName = gameObject.name;
            if (cardName.Contains("(Clone)"))
            {
                cardName = cardName.Replace("(Clone)", "").Trim();
            }
            // Kart adına göre filtrelenmiş kartlardan uygun olanı bul
            List<CardStats> allCards = buildDeck.GetFilteredCards();
            CardStats matchingCard = null;
            // Önce tam eşleşme ara
            matchingCard = allCards.Find(c => c.name == cardName || c.cardName == cardName);
            // Tam eşleşme yoksa, içeren bir kart ara
            if (matchingCard == null)
            {
                matchingCard = allCards.Find(c =>
                    c.name.Contains(cardName) ||
                    cardName.Contains(c.name) ||
                    c.cardName.Contains(cardName) ||
                    cardName.Contains(c.cardName));
            }
            // Eğer hala bulunamadıysa ve kart listesi boş değilse, ilk kartı al
            if (matchingCard == null && allCards.Count > 0)
            {
                Debug.LogWarning($"[CardBuildDeck] {cardName} için uygun CardStats bulunamadı, ilk kart atanıyor: {allCards[0].name}");
                matchingCard = allCards[0];
            }
            if (matchingCard != null)
            {
                cardStats = matchingCard;
                Debug.Log($"[CardBuildDeck] {cardName} için CardStats otomatik olarak atandı: {matchingCard.name}");
            }
            else
            {
                Debug.LogError($"[CardBuildDeck] {cardName} için uygun CardStats bulunamadı ve kart listesi boş!");
            }
        }
        else
        {
            Debug.Log($"[CardBuildDeck] CardStats zaten atanmış: {cardStats.name}");
        }
    }
    private void Start()
    {
        // Kartın lider kartı olup olmadığını kontrol et
        if (cardStats != null)
        {
            // CardStatus.Leader yerine başka bir kontrol kullanabilirsiniz
            // Örneğin: cardStats.isLeader veya cardStats.cardName içinde "Lider" geçiyorsa
            isLeaderCard = IsLeaderCard(cardStats);
        }
        // Kartın oyuncu destesinde olup olmadığını kontrol et
        if (buildDeck != null && cardStats != null)
        {
            isInPlayerDeck = buildDeck.GetPlayerDeck().Contains(cardStats);
        }
        // Eğer bu kart, seçili lider kartı ise özel bir görsel efekt ekle
        if (isLeaderCard && buildDeck != null && buildDeck.GetSelectedLeader() == cardStats)
        {
            // Seçili lider kartı için özel bir görsel efekt ekle
            // Örneğin: Outline bileşeni ekleyebilir veya renk değiştirebilirsiniz
            Outline outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }
            outline.effectColor = Color.yellow;
            outline.effectDistance = new Vector2(3, 3);
        }
    }
    // Tıklama olayını işle
    public void OnPointerClick(PointerEventData eventData)
    {
        // Çift tıklama kontrolü
        if (Time.time - lastClickTime < doubleClickTime)
        {
            OnDoubleClick();
        }
        lastClickTime = Time.time;
    }
    // Çift tıklama işlevi
    private void OnDoubleClick()
    {
        if (cardStats != null && buildDeck != null)
        {
            // Kartın hangi tarafta olduğunu kontrol et
            bool isInDeck = IsInPlayerDeckContent();
            bool isInCollection = IsInCardCollectionContent();
            bool isInLeaderSelection = IsInLeaderSelectionContent();
            bool isInLeaderHolder = IsInLeaderCardHolder();
            // Debug bilgisi ekle
            Debug.Log($"[CardBuildDeck] Konum kontrolleri: isInDeck={isInDeck}, isInCollection={isInCollection}, isInLeaderSelection={isInLeaderSelection}, isInLeaderHolder={isInLeaderHolder}");
            // DeckBuilderUI referansını al
            DeckBuilderUI deckUI = FindObjectOfType<DeckBuilderUI>();
            if (deckUI == null)
            {
                Debug.LogError("[CardBuildDeck] DeckBuilderUI referansı bulunamadı!");
                return;
            }
            // Eğer bu bir lider kartı ise ve lider seçim panelindeyse
            if (isLeaderCard && isInLeaderSelection)
            {
                Debug.Log($"[CardBuildDeck] Lider kartı seçiliyor: {cardStats.name}");
                // Lider kartını seç
                buildDeck.SelectLeader(cardStats);
                return;
            }
            // Eğer bu bir lider kartı ise ve lider kart tutucusundaysa
            if (isLeaderCard && isInLeaderHolder)
            {
                Debug.Log($"[CardBuildDeck] Lider seçim paneli açılıyor");
                // Lider seçim panelini göster
                deckUI.HideLeaderSelection();
                buildDeck.ShowLeaderSelection();
                return;
            }
            // Eğer kart oyuncu destesindeyse, koleksiyona taşı
            if (isInDeck)
            {
                Debug.Log($"[CardBuildDeck] Kart desteden koleksiyona taşınıyor: {cardStats.name}");
                // Kartı desteden çıkar (veri yapısından)
                buildDeck.RemoveCardFromDeck(cardStats);
                // Kartı koleksiyona taşı (görsel olarak)
                transform.SetParent(deckUI.cardCollectionContent);
                // Kartın pozisyonunu ve ölçeğini sıfırla
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                // LayoutGroup'u yeniden hesapla
                LayoutRebuilder.ForceRebuildLayoutImmediate(deckUI.cardCollectionContent as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(deckUI.playerDeckContent as RectTransform);
                // ScrollRect'leri yeniden ayarla
                deckUI.RefreshScrollRects();
            }
            // Eğer kart koleksiyondaysa, desteye taşı
            else if (isInCollection)
            {
                Debug.Log($"[CardBuildDeck] Kart koleksiyondan desteye taşınıyor: {cardStats.name}");
                // Önce destede bu karttan var mı kontrol et
                List<CardStats> playerDeck = buildDeck.GetPlayerDeck();
                bool alreadyInDeck = playerDeck.Contains(cardStats);
                if (!alreadyInDeck)
                {
                    // Kartı desteye ekle (veri yapısına)
                    buildDeck.AddCardToDeck(cardStats);
                    // Kartı desteye taşı (görsel olarak)
                    transform.SetParent(deckUI.playerDeckContent);
                    // Kartın pozisyonunu ve ölçeğini sıfırla
                    transform.localPosition = Vector3.zero;
                    transform.localScale = Vector3.one;
                    // LayoutGroup'u yeniden hesapla
                    LayoutRebuilder.ForceRebuildLayoutImmediate(deckUI.cardCollectionContent as RectTransform);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(deckUI.playerDeckContent as RectTransform);
                    // ScrollRect'leri yeniden ayarla
                    deckUI.RefreshScrollRects();
                }
                else
                {
                    Debug.LogWarning($"[CardBuildDeck] Bu kart zaten destede mevcut: {cardStats.name}");
                    deckUI.ShowError($"Bu kart zaten destede mevcut: {cardStats.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[CardBuildDeck] Kart ne destede ne de koleksiyonda değil: {cardStats.name}, Parent: {transform.parent?.name}");
                // Alternatif çözüm: Eğer kart hiçbir yerde değilse, parent'ına bakarak karar ver
                if (transform.parent != null)
                {
                    string parentName = transform.parent.name.ToLower();
                    if (parentName.Contains("deck") || parentName.Contains("deste"))
                    {
                        Debug.Log($"[CardBuildDeck] Parent ismine göre desteden koleksiyona taşınıyor: {cardStats.name}");
                        // Kartı desteden çıkar (veri yapısından)
                        buildDeck.RemoveCardFromDeck(cardStats);
                        // Kartı koleksiyona taşı (görsel olarak)
                        transform.SetParent(deckUI.cardCollectionContent);
                        // Kartın pozisyonunu ve ölçeğini sıfırla
                        transform.localPosition = Vector3.zero;
                        transform.localScale = Vector3.one;
                        // LayoutGroup'u yeniden hesapla
                        LayoutRebuilder.ForceRebuildLayoutImmediate(deckUI.cardCollectionContent as RectTransform);
                        LayoutRebuilder.ForceRebuildLayoutImmediate(deckUI.playerDeckContent as RectTransform);
                        // ScrollRect'leri yeniden ayarla
                        deckUI.RefreshScrollRects();
                    }
                    else
                    {
                        Debug.Log($"[CardBuildDeck] Parent ismine göre koleksiyondan desteye taşınıyor: {cardStats.name}");
                        // Önce destede bu karttan var mı kontrol et
                        List<CardStats> deck = buildDeck.GetPlayerDeck();
                        bool alreadyInDeck = deck.Contains(cardStats);
                        if (!alreadyInDeck)
                        {
                            // Kartı desteye ekle (veri yapısına)
                            buildDeck.AddCardToDeck(cardStats);
                            // Kartı desteye taşı (görsel olarak)
                            transform.SetParent(deckUI.playerDeckContent);
                            // Kartın pozisyonunu ve ölçeğini sıfırla
                            transform.localPosition = Vector3.zero;
                            transform.localScale = Vector3.one;
                            // LayoutGroup'u yeniden hesapla
                            LayoutRebuilder.ForceRebuildLayoutImmediate(deckUI.cardCollectionContent as RectTransform);
                            LayoutRebuilder.ForceRebuildLayoutImmediate(deckUI.playerDeckContent as RectTransform);
                            // ScrollRect'leri yeniden ayarla
                            deckUI.RefreshScrollRects();
                        }
                        else
                        {
                            Debug.LogWarning($"[CardBuildDeck] Bu kart zaten destede mevcut: {cardStats.name}");
                            deckUI.ShowError($"Bu kart zaten destede mevcut: {cardStats.name}");
                        }
                    }
                }
            }
            // UI'ı güncelle
            buildDeck.UpdateUI();
        }
    }
    // Kartın destede olup olmadığını kontrol et
    private bool IsCardInDeck()
    {
        if (buildDeck != null && cardStats != null)
        {
            List<CardStats> playerDeck = buildDeck.GetPlayerDeck();
            return playerDeck.Contains(cardStats);
        }
        return false;
    }
    // Kartın lider kartı olup olmadığını kontrol et
    private bool IsLeaderCard(CardStats stats)
    {
        // Burada kendi lider kart kontrolünüzü yapabilirsiniz
        // Örneğin: Kart adında "Lider" geçiyorsa veya özel bir tag'i varsa
        return stats.name.Contains("Leader") || stats.name.Contains("Lider");
    }
    // Kartın oyuncu destesi içeriğinde olup olmadığını kontrol et
    private bool IsInPlayerDeckContent()
    {
        if (transform.parent == null)
            return false;
        // DeckBuilderUI'daki playerDeckContent referansını bul
        DeckBuilderUI deckUI = FindObjectOfType<DeckBuilderUI>();
        if (deckUI == null)
            return false;
        // Transform'un parent'ı playerDeckContent mi kontrol et
        bool exactMatch = transform.parent == deckUI.playerDeckContent;
        // Eğer tam eşleşme yoksa, parent'ın ismini kontrol et
        if (!exactMatch)
        {
            string parentName = transform.parent.name.ToLower();
            return parentName.Contains("deck") || parentName.Contains("deste") || parentName.Contains("player");
        }
        return exactMatch;
    }
    // Kartın koleksiyon içeriğinde olup olmadığını kontrol et
    private bool IsInCardCollectionContent()
    {
        if (transform.parent == null)
            return false;
        // DeckBuilderUI'daki cardCollectionContent referansını bul
        DeckBuilderUI deckUI = FindObjectOfType<DeckBuilderUI>();
        if (deckUI == null)
            return false;
        // Transform'un parent'ı cardCollectionContent mi kontrol et
        bool exactMatch = transform.parent == deckUI.cardCollectionContent;
        // Eğer tam eşleşme yoksa, parent'ın ismini kontrol et
        if (!exactMatch)
        {
            string parentName = transform.parent.name.ToLower();
            return parentName.Contains("collection") || parentName.Contains("koleksiyon");
        }
        return exactMatch;
    }
    // Kartın lider seçim panelinde olup olmadığını kontrol et
    private bool IsInLeaderSelectionContent()
    {
        if (transform.parent == null)
            return false;
        // DeckBuilderUI'daki leaderSelectionContent referansını bul
        DeckBuilderUI deckUI = FindObjectOfType<DeckBuilderUI>();
        if (deckUI == null)
            return false;
        // Transform'un parent'ı leaderSelectionContent mi kontrol et
        bool exactMatch = transform.parent == deckUI.leaderSelectionContent;
        // Eğer tam eşleşme yoksa, parent'ın ismini kontrol et
        if (!exactMatch)
        {
            string parentName = transform.parent.name.ToLower();
            return parentName.Contains("leader") || parentName.Contains("lider") || parentName.Contains("selection");
        }
        return exactMatch;
    }
    // Kartın lider kart tutucusunda olup olmadığını kontrol et
    private bool IsInLeaderCardHolder()
    {
        if (transform.parent == null)
            return false;
        // DeckBuilderUI'daki leaderCardHolder referansını bul
        DeckBuilderUI deckUI = FindObjectOfType<DeckBuilderUI>();
        if (deckUI == null)
            return false;
        // Transform'un parent'ı leaderCardHolder mi kontrol et
        bool exactMatch = transform.parent == deckUI.leaderCardHolder;
        // Eğer tam eşleşme yoksa, parent'ın ismini kontrol et
        if (!exactMatch)
        {
            string parentName = transform.parent.name.ToLower();
            return parentName.Contains("leader") || parentName.Contains("lider") || parentName.Contains("holder");
        }
        return exactMatch;
    }
    // CardStats'ı dışarıdan ayarlamak için
    public void SetCardStats(CardStats stats)
    {
        cardStats = stats;
        if (stats != null)
        {
            isLeaderCard = IsLeaderCard(stats);
            // Kartın oyuncu destesinde olup olmadığını kontrol et
            if (buildDeck != null)
            {
                isInPlayerDeck = buildDeck.GetPlayerDeck().Contains(stats);
            }
        }
    }
}
