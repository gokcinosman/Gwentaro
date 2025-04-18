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
        // Kartın oyuncu destesinde olup olmadığını kontrol et
        if (buildDeck != null && cardStats != null)
        {
            isInPlayerDeck = buildDeck.GetPlayerDeck().Contains(cardStats);
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
            DeckBuilderUI deckUI = FindObjectOfType<DeckBuilderUI>();
            if (deckUI == null)
            {
                Debug.LogError("[CardBuildDeck] DeckBuilderUI referansı bulunamadı!");
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
            buildDeck.SaveDeck();
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
    // Kartın lider kart tutucusunda olup olmadığını kontrol et
    // CardStats'ı dışarıdan ayarlamak için
    public void SetCardStats(CardStats stats)
    {
        cardStats = stats;
        if (stats != null)
        {
            if (buildDeck != null)
            {
                isInPlayerDeck = buildDeck.GetPlayerDeck().Contains(stats);
            }
        }
    }
}
