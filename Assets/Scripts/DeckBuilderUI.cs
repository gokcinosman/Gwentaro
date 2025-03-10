using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
public class DeckBuilderUI : MonoBehaviour
{
    [Header("Ana Panel")]
    [SerializeField] private GameObject deckBuilderPanel;
    [Header("Kart Koleksiyonu")]
    [SerializeField] private GameObject cardCollectionPanel;
    [SerializeField] public Transform cardCollectionContent;
    [SerializeField] private ScrollRect cardCollectionScrollRect;
    [Header("Oyuncu Destesi")]
    [SerializeField] private GameObject playerDeckPanel;
    [SerializeField] public Transform playerDeckContent;
    [SerializeField] private TextMeshProUGUI deckInfoText;
    [Header("Lider Kartı")]
    [SerializeField] private GameObject leaderCardPanel;
    [SerializeField] public Transform leaderCardHolder;
    [SerializeField] private GameObject leaderSelectionPanel;
    [SerializeField] public Transform leaderSelectionContent;
    [Header("Filtreler")]
    [SerializeField] private TMP_Dropdown factionDropdown;
    [SerializeField] private TMP_Dropdown cardTypeDropdown;
    [SerializeField] private TMP_InputField searchInputField;
    [Header("Butonlar")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI errorText;
    [Header("ScrollView Ayarları")]
    [SerializeField] private bool limitScrolling = true; // Kaydırmayı sınırlandırma seçeneği
    [SerializeField] private float scrollPadding = 50f; // Kaydırma sınırı için ek boşluk
    private BuildDeck buildDeck;
    private ContentSizeFitter cardCollectionContentSizeFitter;
    private ContentSizeFitter playerDeckContentSizeFitter;
    private ContentSizeFitter leaderSelectionContentSizeFitter;
    private void Awake()
    {
        buildDeck = GetComponent<BuildDeck>();
        if (buildDeck == null)
        {
            Debug.LogError("[DeckBuilderUI] BuildDeck bileşeni bulunamadı!");
        }
        // ContentSizeFitter bileşenlerini al
        cardCollectionContentSizeFitter = cardCollectionContent?.GetComponent<ContentSizeFitter>();
        playerDeckContentSizeFitter = playerDeckContent?.GetComponent<ContentSizeFitter>();
        // leaderSelectionContentSizeFitter = leaderSelectionContent?.GetComponent<ContentSizeFitter>();
    }
    private void Start()
    {
        // Butonları ayarla
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(() => buildDeck.SaveDeck());
        }
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(() => buildDeck.ClearDeck());
        }
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => buildDeck.GoBack());
        }
        // Dropdown'ları ayarla
        if (factionDropdown != null)
        {
            factionDropdown.ClearOptions();
            List<string> factions = new List<string> { "Tümü", "Northern Realms", "Nilfgaard", "Monsters", "Scoia'tael" };
            factionDropdown.AddOptions(factions);
            factionDropdown.onValueChanged.AddListener((index) => buildDeck.OnFactionChanged(index));
        }
        if (cardTypeDropdown != null)
        {
            cardTypeDropdown.ClearOptions();
            List<string> cardTypes = new List<string> { "Tümü", "Melee", "Ranged", "Siege" };
            cardTypeDropdown.AddOptions(cardTypes);
            cardTypeDropdown.onValueChanged.AddListener((index) => buildDeck.OnCardTypeChanged(index));
        }
        // Arama alanını ayarla
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(OnSearchTextChanged);
        }
        // Lider seçim panelini başlangıçta gizle
        if (leaderSelectionPanel != null)
        {
            leaderSelectionPanel.SetActive(false);
        }
        // ScrollRect'leri ayarla
        SetupScrollRects();
    }
    private void SetupScrollRects()
    {
        if (cardCollectionScrollRect != null)
        {
            // ScrollRect'in içeriğinin değişimini dinle
            cardCollectionScrollRect.onValueChanged.AddListener(OnCardCollectionScrollValueChanged);
        }
        // Diğer ScrollRect'ler için de benzer ayarlamalar yapılabilir
    }
    private void OnCardCollectionScrollValueChanged(Vector2 scrollPosition)
    {
        if (!limitScrolling || cardCollectionScrollRect == null || cardCollectionContent == null)
            return;
        // İçerik boyutunu güncelle
        UpdateContentSize(cardCollectionContent, cardCollectionContentSizeFitter);
        // Kaydırma sınırlarını ayarla
        LimitScrolling(cardCollectionScrollRect, cardCollectionContent);
    }
    private void UpdateContentSize(Transform content, ContentSizeFitter sizeFitter)
    {
        // ContentSizeFitter varsa, bir kare boyutunu güncelle
        if (sizeFitter != null)
        {
            Canvas.ForceUpdateCanvases();
            sizeFitter.enabled = false;
            sizeFitter.enabled = true;
        }
    }
    private void LimitScrolling(ScrollRect scrollRect, Transform content)
    {
        // ScrollRect ve içerik RectTransform'larını al
        RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        RectTransform contentRectTransform = content.GetComponent<RectTransform>();
        if (scrollRectTransform == null || contentRectTransform == null)
            return;
        // İçerik boyutu ve görünüm alanı boyutu
        float contentHeight = contentRectTransform.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        // İçerik boyutu görünüm alanından küçükse, kaydırmayı sınırla
        if (contentHeight <= viewportHeight)
        {
            // İçeriği görünüm alanının üstüne hizala
            Vector2 newPosition = contentRectTransform.anchoredPosition;
            newPosition.y = 0;
            contentRectTransform.anchoredPosition = newPosition;
            return;
        }
        // Kaydırma sınırlarını hesapla
        float maxY = 0; // Üst sınır
        float minY = -(contentHeight - viewportHeight); // Alt sınır
        // Ek boşluk ekle
        minY -= scrollPadding;
        // Mevcut pozisyonu al ve sınırla
        Vector2 position = contentRectTransform.anchoredPosition;
        position.y = Mathf.Clamp(position.y, minY, maxY);
        contentRectTransform.anchoredPosition = position;
    }
    // Arama metni değiştiğinde
    private void OnSearchTextChanged(string searchText)
    {
        if (buildDeck != null)
        {
            buildDeck.FilterBySearchText(searchText);
        }
    }
    // Arama filtresini temizle
    public void ClearSearchFilter()
    {
        if (searchInputField != null)
        {
            searchInputField.text = "";
        }
        if (buildDeck != null)
        {
            buildDeck.FilterBySearchText("");
        }
    }
    // Deste oluşturma ekranını aç
    public void OpenDeckBuilder()
    {
        deckBuilderPanel.SetActive(true);
        // ScrollRect'leri yeniden ayarla
        StartCoroutine(DelayedScrollRectSetup());
    }
    // Deste oluşturma ekranını kapat
    public void CloseDeckBuilder()
    {
        deckBuilderPanel.SetActive(false);
    }
    // İçerik değiştikten sonra ScrollRect'i yeniden ayarlamak için gecikmeli çağrı
    private IEnumerator DelayedScrollRectSetup()
    {
        // İçeriğin güncellenmesi için bir kare bekle
        yield return null;
        // Canvas'ı güncelle
        Canvas.ForceUpdateCanvases();
        // ContentSizeFitter'ları güncelle
        if (cardCollectionContentSizeFitter != null)
        {
            cardCollectionContentSizeFitter.enabled = false;
            cardCollectionContentSizeFitter.enabled = true;
        }
        if (playerDeckContentSizeFitter != null)
        {
            playerDeckContentSizeFitter.enabled = false;
            playerDeckContentSizeFitter.enabled = true;
        }
        if (leaderSelectionContentSizeFitter != null)
        {
            leaderSelectionContentSizeFitter.enabled = false;
            leaderSelectionContentSizeFitter.enabled = true;
        }
        // Bir kare daha bekle
        yield return null;
        // Kaydırma sınırlarını ayarla
        if (cardCollectionScrollRect != null && cardCollectionContent != null)
        {
            LimitScrolling(cardCollectionScrollRect, cardCollectionContent);
        }
    }
    // Public metot: ScrollRect'leri yeniden ayarla
    public void RefreshScrollRects()
    {
        StartCoroutine(DelayedScrollRectSetup());
    }
    // Update metodunda sürekli olarak kaydırma sınırlarını kontrol et
    private void Update()
    {
        if (limitScrolling && cardCollectionScrollRect != null && cardCollectionContent != null)
        {
            LimitScrolling(cardCollectionScrollRect, cardCollectionContent);
        }
    }
    // BuildDeck sınıfı için UI güncelleme metodları
    // Kart koleksiyonunu güncelle
    public void UpdateCardCollection(List<CardStats> filteredCards, GameObject cardPrefab, Action<CardStats> onCardClicked)
    {
        // Önce mevcut kartları temizle
        foreach (Transform child in cardCollectionContent)
        {
            Destroy(child.gameObject);
        }
        // Filtrelenmiş kartları göster
        foreach (CardStats cardStats in filteredCards)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardCollectionContent);
            Card card = cardObj.GetComponent<Card>();
            // Kart bilgilerini ayarla
            if (card != null)
            {
                card.cardStats = cardStats;
                // Kart görselini güncelle
                if (card.cardVisual != null)
                {
                    card.cardVisual.UpdateVisual(cardStats);
                }
                // Karta tıklama olayını ekle
                Button cardButton = cardObj.GetComponent<Button>();
                if (cardButton != null)
                {
                    cardButton.onClick.AddListener(() => onCardClicked(cardStats));
                }
                // CardBuildDeck bileşenini ekle (eğer yoksa)
                CardBuildDeck cardBuildDeck = cardObj.GetComponent<CardBuildDeck>();
                if (cardBuildDeck == null)
                {
                    cardBuildDeck = cardObj.AddComponent<CardBuildDeck>();
                }
                // CardStats referansını doğrudan ayarla
                cardBuildDeck.SetCardStats(cardStats);
            }
        }
        // ScrollRect'i yeniden ayarla
        RefreshScrollRects();
    }
    // Oyuncu destesini güncelle
    public void UpdatePlayerDeck(List<CardStats> playerDeck, GameObject cardPrefab, Action<CardStats> onCardClicked)
    {
        // Önce mevcut kartları temizle
        foreach (Transform child in playerDeckContent)
        {
            Destroy(child.gameObject);
        }
        // Destedeki kartları göster
        foreach (CardStats cardStats in playerDeck)
        {
            GameObject cardObj = Instantiate(cardPrefab, playerDeckContent);
            Card card = cardObj.GetComponent<Card>();
            // Kart bilgilerini ayarla
            if (card != null)
            {
                card.cardStats = cardStats;
                // Kart görselini güncelle
                if (card.cardVisual != null)
                {
                    card.cardVisual.UpdateVisual(cardStats);
                }
                // Karta tıklama olayını ekle
                Button cardButton = cardObj.GetComponent<Button>();
                if (cardButton != null)
                {
                    cardButton.onClick.AddListener(() => onCardClicked(cardStats));
                }
                // CardBuildDeck bileşenini ekle (eğer yoksa)
                CardBuildDeck cardBuildDeck = cardObj.GetComponent<CardBuildDeck>();
                if (cardBuildDeck == null)
                {
                    cardBuildDeck = cardObj.AddComponent<CardBuildDeck>();
                }
                // CardStats referansını doğrudan ayarla
                cardBuildDeck.SetCardStats(cardStats);
            }
        }
        // ScrollRect'i yeniden ayarla
        RefreshScrollRects();
    }
    // Deste bilgilerini güncelle
    public void UpdateDeckInfo(List<CardStats> playerDeck, int maxDeckSize, int minDeckSize, CardStats selectedLeader)
    {
        // Deste istatistiklerini hesapla
        int totalCards = playerDeck.Count;
        int unitCards = playerDeck.Count(c => c.cardStatue == CardStatus.Unit);
        int heroCards = playerDeck.Count(c => c.cardStatue == CardStatus.Hero);
        int specialCards = playerDeck.Count(c => c.cardStatue == CardStatus.Special);
        int totalPower = playerDeck.Sum(c => c.cardValue);
        // Deste bilgilerini güncelle
        if (deckInfoText != null)
        {
            deckInfoText.text = $"Toplam Kart: {totalCards}/{maxDeckSize}\n" +
                               $"Unit Kart: {unitCards}\n" +
                               $"Hero Kart: {heroCards}\n" +
                               $"Special Kart: {specialCards}\n" +
                               $"Toplam Güç: {totalPower}";
        }
        // Kaydet butonunu güncelle
        if (saveButton != null)
        {
            saveButton.interactable = totalCards >= minDeckSize && selectedLeader != null;
        }
    }
    // Lider kartını güncelle
    public void UpdateLeaderCard(CardStats leaderCard, GameObject cardPrefab, Action onLeaderClicked)
    {
        // Önce mevcut lider kartını temizle
        foreach (Transform child in leaderCardHolder)
        {
            Destroy(child.gameObject);
        }
        // Lider kartı yoksa çık
        if (leaderCard == null)
        {
            return;
        }
        // Lider kartını oluştur
        GameObject cardObj = Instantiate(cardPrefab, leaderCardHolder);
        Card card = cardObj.GetComponent<Card>();
        // Kart bilgilerini ayarla
        if (card != null)
        {
            card.cardStats = leaderCard;
            // Kart görselini güncelle
            if (card.cardVisual != null)
            {
                card.cardVisual.UpdateVisual(leaderCard);
            }
            // Karta tıklama olayını ekle
            Button cardButton = cardObj.GetComponent<Button>();
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(() => onLeaderClicked());
            }
            // CardBuildDeck bileşenini ekle (eğer yoksa)
            CardBuildDeck cardBuildDeck = cardObj.GetComponent<CardBuildDeck>();
            if (cardBuildDeck == null)
            {
                cardBuildDeck = cardObj.AddComponent<CardBuildDeck>();
            }
            // CardStats referansını doğrudan ayarla
            cardBuildDeck.SetCardStats(leaderCard);
        }
    }
    // Lider seçim panelini göster
    public void ShowLeaderSelection(List<CardStats> leaderCards, GameObject cardPrefab, Action<CardStats> onLeaderSelected)
    {
        // Lider seçim panelini göster
        if (leaderSelectionPanel != null)
        {
            leaderSelectionPanel.SetActive(true);
        }
        // Önce mevcut liderleri temizle
        foreach (Transform child in leaderSelectionContent)
        {
            Destroy(child.gameObject);
        }
        // Lider kartlarını göster
        foreach (CardStats leaderCard in leaderCards)
        {
            GameObject cardObj = Instantiate(cardPrefab, leaderSelectionContent);
            Card card = cardObj.GetComponent<Card>();
            // Kart bilgilerini ayarla
            if (card != null)
            {
                card.cardStats = leaderCard;
                // Kart görselini güncelle
                if (card.cardVisual != null)
                {
                    card.cardVisual.UpdateVisual(leaderCard);
                }
                // Karta tıklama olayını ekle
                Button cardButton = cardObj.GetComponent<Button>();
                if (cardButton != null)
                {
                    cardButton.onClick.AddListener(() => onLeaderSelected(leaderCard));
                }
                // CardBuildDeck bileşenini ekle (eğer yoksa)
                CardBuildDeck cardBuildDeck = cardObj.GetComponent<CardBuildDeck>();
                if (cardBuildDeck == null)
                {
                    cardBuildDeck = cardObj.AddComponent<CardBuildDeck>();
                }
                // CardStats referansını doğrudan ayarla
                cardBuildDeck.SetCardStats(leaderCard);
            }
        }
        // ScrollRect'i yeniden ayarla
        RefreshScrollRects();
    }
    // Lider seçim panelini gizle
    public void HideLeaderSelection()
    {
        if (leaderSelectionPanel != null)
        {
            leaderSelectionPanel.SetActive(false);
        }
    }
    // Hata mesajını göster
    public void ShowError(string message, bool isError = true)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = isError ? Color.red : Color.green;
        }
    }
    // Hata mesajını temizle
    public void ClearError()
    {
        if (errorText != null)
        {
            errorText.text = "";
        }
    }
}