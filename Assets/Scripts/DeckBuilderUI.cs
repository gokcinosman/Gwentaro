using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DeckBuilderUI : MonoBehaviour
{
    [Header("Ana Panel")]
    [SerializeField] private GameObject deckBuilderPanel;
    [Header("Kart Koleksiyonu")]
    [SerializeField] private GameObject cardCollectionPanel;
    [SerializeField] private Transform cardCollectionContent;
    [SerializeField] private ScrollRect cardCollectionScrollRect;
    [Header("Oyuncu Destesi")]
    [SerializeField] private GameObject playerDeckPanel;
    [SerializeField] private Transform playerDeckContent;
    [SerializeField] private TextMeshProUGUI deckInfoText;
    [Header("Lider Kartı")]
    [SerializeField] private GameObject leaderCardPanel;
    [SerializeField] private Transform leaderCardHolder;
    [SerializeField] private GameObject leaderSelectionPanel;
    [SerializeField] private Transform leaderSelectionContent;
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
        leaderSelectionContentSizeFitter = leaderSelectionContent?.GetComponent<ContentSizeFitter>();
    }
    private void Start()
    {
        // UI elemanlarını BuildDeck sınıfına bağla
        ConnectUIToBuildDeck();
        // Lider seçim panelini başlangıçta gizle
        if (leaderSelectionPanel != null)
        {
            leaderSelectionPanel.SetActive(false);
        }
        // Arama alanını ayarla
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(OnSearchTextChanged);
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
    private void ConnectUIToBuildDeck()
    {
        if (buildDeck == null)
            return;
        // Reflection kullanarak BuildDeck sınıfındaki SerializeField değişkenlerine UI elemanlarını ata
        System.Reflection.FieldInfo[] fields = typeof(BuildDeck).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (System.Reflection.FieldInfo field in fields)
        {
            // SerializeField attribute'u olan alanları bul
            if (field.GetCustomAttributes(typeof(SerializeField), false).Length > 0)
            {
                // Alan adına göre UI elemanını ata
                switch (field.Name)
                {
                    case "cardCollectionContent":
                        field.SetValue(buildDeck, cardCollectionContent);
                        break;
                    case "cardCollectionScrollRect":
                        field.SetValue(buildDeck, cardCollectionScrollRect);
                        break;
                    case "playerDeckContent":
                        field.SetValue(buildDeck, playerDeckContent);
                        break;
                    case "deckInfoText":
                        field.SetValue(buildDeck, deckInfoText);
                        break;
                    case "leaderCardHolder":
                        field.SetValue(buildDeck, leaderCardHolder);
                        break;
                    case "leaderSelectionPanel":
                        field.SetValue(buildDeck, leaderSelectionPanel);
                        break;
                    case "leaderSelectionContent":
                        field.SetValue(buildDeck, leaderSelectionContent);
                        break;
                    case "factionDropdown":
                        field.SetValue(buildDeck, factionDropdown);
                        break;
                    case "cardTypeDropdown":
                        field.SetValue(buildDeck, cardTypeDropdown);
                        break;
                    case "saveButton":
                        field.SetValue(buildDeck, saveButton);
                        break;
                    case "clearButton":
                        field.SetValue(buildDeck, clearButton);
                        break;
                    case "backButton":
                        field.SetValue(buildDeck, backButton);
                        break;
                    case "errorText":
                        field.SetValue(buildDeck, errorText);
                        break;
                }
            }
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
    // Arama metni değiştiğinde
    private void OnSearchTextChanged(string searchText)
    {
        if (buildDeck != null)
        {
            buildDeck.FilterBySearchText(searchText);
        }
        // İçerik değiştiğinde ScrollRect'i yeniden ayarla
        StartCoroutine(DelayedScrollRectSetup());
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
        // İçerik değiştiğinde ScrollRect'i yeniden ayarla
        StartCoroutine(DelayedScrollRectSetup());
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
}