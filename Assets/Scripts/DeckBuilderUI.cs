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
    [SerializeField] private ScrollRect playerDeckScrollRect;
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
    }
    private void OnSearchTextChanged(string searchText)
    {
        if (buildDeck != null)
        {
            buildDeck.FilterBySearchText(searchText);
        }
    }
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
        // Bir kare daha bekle
        yield return null;
        // İçerik boyutlarını kartlara göre ayarla
        AdjustContentSize(cardCollectionContent, cardCollectionScrollRect);
        // Player deck için ScrollRect bileşenini kullan
        if (playerDeckScrollRect == null)
        {
            // Eğer atanmamışsa, bileşeni bulmaya çalış
            playerDeckScrollRect = playerDeckPanel.GetComponent<ScrollRect>();
            if (playerDeckScrollRect == null)
            {
                // Eğer panelde yoksa, içerik üst öğesinde ara
                playerDeckScrollRect = playerDeckContent.parent.GetComponent<ScrollRect>();
            }
        }
        if (playerDeckScrollRect != null)
        {
            AdjustContentSize(playerDeckContent, playerDeckScrollRect);
            Debug.Log($"[DeckBuilderUI] Deste ScrollRect ayarlandı: İçerik yüksekliği = {playerDeckContent.GetComponent<RectTransform>().sizeDelta.y}");
        }
        else
        {
            Debug.LogError("[DeckBuilderUI] Deste için ScrollRect bulunamadı!");
        }
        // Canvas'ı tekrar güncelle
        Canvas.ForceUpdateCanvases();
    }
    // İçerik boyutunu kartlara göre ayarlayan yeni metod
    private void AdjustContentSize(Transform content, ScrollRect scrollRect)
    {
        if (content == null || scrollRect == null || content.childCount == 0)
            return;
        // Grid Layout Group varsa
        GridLayoutGroup gridLayout = content.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            int childCount = content.childCount;
            float cellWidth = gridLayout.cellSize.x;
            float cellHeight = gridLayout.cellSize.y;
            float spacingX = gridLayout.spacing.x;
            float spacingY = gridLayout.spacing.y;
            // Bir satırdaki maksimum kart sayısını hesapla
            float viewportWidth = scrollRect.viewport.rect.width;
            int cardsPerRow = Mathf.Max(1, Mathf.FloorToInt((viewportWidth - gridLayout.padding.left - gridLayout.padding.right + spacingX) / (cellWidth + spacingX)));
            // Toplam satır sayısını hesapla
            int rowCount = Mathf.CeilToInt((float)childCount / cardsPerRow);
            // İçerik yüksekliğini hesapla
            float contentHeight = (rowCount * cellHeight) + ((rowCount - 1) * spacingY) + gridLayout.padding.top + gridLayout.padding.bottom;
            // RectTransform'u ayarla
            RectTransform rectTransform = content.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 sizeDelta = rectTransform.sizeDelta;
                sizeDelta.y = contentHeight;
                rectTransform.sizeDelta = sizeDelta;
            }
        }
        // Vertical Layout Group varsa
        else
        {
            VerticalLayoutGroup verticalLayout = content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout != null)
            {
                float totalHeight = 0;
                // Tüm çocukların yüksekliğini topla
                for (int i = 0; i < content.childCount; i++)
                {
                    RectTransform childRect = content.GetChild(i).GetComponent<RectTransform>();
                    if (childRect != null)
                    {
                        totalHeight += childRect.rect.height;
                    }
                }
                // Spacing'i ekle
                totalHeight += verticalLayout.spacing * (content.childCount - 1);
                // Padding'i ekle
                totalHeight += verticalLayout.padding.top + verticalLayout.padding.bottom;
                // RectTransform'u ayarla
                RectTransform rectTransform = content.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector2 sizeDelta = rectTransform.sizeDelta;
                    sizeDelta.y = totalHeight;
                    rectTransform.sizeDelta = sizeDelta;
                }
            }
        }
    }
    public void RefreshScrollRects()
    {
        StartCoroutine(DelayedScrollRectSetup());
    }
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
            // Kart prefabını oluştur
            GameObject cardObj = Instantiate(cardPrefab, cardCollectionContent);
            // Kart adını ayarla (CardBuildDeck'in kart adına göre eşleştirme yapabilmesi için)
            cardObj.name = cardStats.name;
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
            // Debug bilgisi
            Debug.Log($"[DeckBuilderUI] Kart oluşturuldu: {cardStats.name}, CardBuildDeck atandı: {cardBuildDeck != null}");
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
            // Kart prefabını oluştur
            GameObject cardObj = Instantiate(cardPrefab, playerDeckContent);
            // Kart adını ayarla (CardBuildDeck'in kart adına göre eşleştirme yapabilmesi için)
            cardObj.name = cardStats.name;
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
            // Debug bilgisi
            Debug.Log($"[DeckBuilderUI] Deste kartı oluşturuldu: {cardStats.name}, CardBuildDeck atandı: {cardBuildDeck != null}");
        }
        // ScrollRect'i yeniden ayarla
        RefreshScrollRects();
        // Debug bilgisi ekle
        Debug.Log($"[DeckBuilderUI] Deste güncellendi: {playerDeck.Count} kart, ScrollRect yenilendi");
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