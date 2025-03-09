using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using TMPro;
public class BuildDeck : MonoBehaviourPunCallbacks
{
    // oyun başında destemizi oluşturmak için bir ekran olacak.
    // bu ekranda sol tarafta tüm kartlar olacak ve sağ tarafta da bizim destemiz olacak.
    // orta taraftada lider kartı bulunacak. Bu lider kartına tıklandığında da ona özel bir seçim ekranı gelecek.
    // lider kartının alt kısmında desteyle ilgili bilgiler bulunacak.
    // destedeki toplam kart sayısı, destedeki toplam unit card sayısı, destenin toplam gücü(value), hero card sayısı, special card sayısı.
    [Header("Kart Koleksiyonu")]
    [SerializeField] private Transform cardCollectionContent; // Sol taraftaki tüm kartların içeriği
    [SerializeField] private GameObject cardPrefab; // Kart prefabı
    [SerializeField] private ScrollRect cardCollectionScrollRect; // Kart koleksiyonu scroll rect
    [SerializeField] private TMP_Dropdown factionDropdown; // Faction seçimi için dropdown
    [SerializeField] private TMP_Dropdown cardTypeDropdown; // Kart tipi filtresi için dropdown
    [Header("Oyuncu Destesi")]
    [SerializeField] private Transform playerDeckContent; // Sağ taraftaki oyuncu destesi içeriği
    [SerializeField] private TextMeshProUGUI deckInfoText; // Deste bilgilerini gösteren metin
    [SerializeField] private int maxDeckSize = 40; // Maksimum deste boyutu
    [SerializeField] private int minDeckSize = 25; // Minimum deste boyutu
    [Header("Lider Kartı")]
    [SerializeField] private Transform leaderCardHolder; // Lider kartının tutulduğu yer
    [SerializeField] private GameObject leaderSelectionPanel; // Lider seçim paneli
    [SerializeField] private Transform leaderSelectionContent; // Lider seçim içeriği
    [Header("UI Elemanları")]
    [SerializeField] private Button saveButton; // Desteyi kaydetme butonu
    [SerializeField] private Button clearButton; // Desteyi temizleme butonu
    [SerializeField] private Button backButton; // Geri dönme butonu
    [SerializeField] private TextMeshProUGUI errorText; // Hata mesajları için metin
    // Özel Değişkenler
    private List<CardStats> allCards = new List<CardStats>(); // Tüm kartlar
    private List<CardStats> filteredCards = new List<CardStats>(); // Filtrelenmiş kartlar
    private List<CardStats> playerDeck = new List<CardStats>(); // Oyuncu destesi
    private CardStats selectedLeader; // Seçilen lider kartı
    private string currentFaction = "Tümü"; // Şu anki seçili faction
    private CardType currentCardType = CardType.None; // Şu anki seçili kart tipi
    private string currentSearchText = ""; // Şu anki arama metni
    private void Awake()
    {
        // Tüm kartları yükle
        LoadAllCards();
        // UI elemanlarını ayarla
        SetupUI();
    }
    private void Start()
    {
        // Kart koleksiyonunu göster
        ShowCardCollection();
        // Lider seçim panelini gizle
        leaderSelectionPanel.SetActive(false);
        // Deste bilgilerini güncelle
        UpdateDeckInfo();
    }
    private void LoadAllCards()
    {
        // Tüm CardStats ScriptableObject'lerini Resources klasöründen yükle
        CardStats[] cards = Resources.LoadAll<CardStats>("Cards");
        allCards.AddRange(cards);
        filteredCards = new List<CardStats>(allCards);
    }
    private void SetupUI()
    {
        // Faction dropdown'ını ayarla
        factionDropdown.ClearOptions();
        List<string> factions = new List<string> { "Tümü", "Northern Realms", "Nilfgaard", "Monsters", "Scoia'tael" };
        factionDropdown.AddOptions(factions);
        factionDropdown.onValueChanged.AddListener(OnFactionChanged);
        // Kart tipi dropdown'ını ayarla
        cardTypeDropdown.ClearOptions();
        List<string> cardTypes = new List<string> { "Tümü", "Melee", "Ranged", "Siege" };
        cardTypeDropdown.AddOptions(cardTypes);
        cardTypeDropdown.onValueChanged.AddListener(OnCardTypeChanged);
        // Butonları ayarla
        saveButton.onClick.AddListener(SaveDeck);
        clearButton.onClick.AddListener(ClearDeck);
        backButton.onClick.AddListener(GoBack);
        // Hata metnini temizle
        errorText.text = "";
    }
    private void ShowCardCollection()
    {
        // Önce mevcut kartları temizle
        foreach (Transform child in cardCollectionContent)
        {
            Destroy(child.gameObject);
        }
        // Filtrelenmiş kartları göster
        foreach (CardStats cardStats in filteredCards)
        {
            // Lider kartlarını koleksiyonda gösterme
            if (cardStats.cardStatue == CardStatus.Leader)
                continue;
            GameObject cardObj = Instantiate(cardPrefab, cardCollectionContent);
            Card card = cardObj.GetComponent<Card>();
            // Kart bilgilerini ayarla
            if (card != null && card.cardStats != null)
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
                    cardButton.onClick.AddListener(() => AddCardToDeck(cardStats));
                }
            }
        }
        // ScrollRect'i yeniden ayarla
        DeckBuilderUI deckBuilderUI = GetComponent<DeckBuilderUI>();
        if (deckBuilderUI != null)
        {
            deckBuilderUI.RefreshScrollRects();
        }
    }
    private void ShowLeaderSelection()
    {
        // Lider seçim panelini göster
        leaderSelectionPanel.SetActive(true);
        // Önce mevcut liderleri temizle
        foreach (Transform child in leaderSelectionContent)
        {
            Destroy(child.gameObject);
        }
        // Sadece lider kartlarını göster
        foreach (CardStats cardStats in allCards)
        {
            if (cardStats.cardStatue == CardStatus.Leader)
            {
                GameObject cardObj = Instantiate(cardPrefab, leaderSelectionContent);
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
                        cardButton.onClick.AddListener(() => SelectLeader(cardStats));
                    }
                }
            }
        }
        // ScrollRect'i yeniden ayarla
        DeckBuilderUI deckBuilderUI = GetComponent<DeckBuilderUI>();
        if (deckBuilderUI != null)
        {
            deckBuilderUI.RefreshScrollRects();
        }
    }
    private void SelectLeader(CardStats leaderCard)
    {
        selectedLeader = leaderCard;
        // Lider seçim panelini kapat
        leaderSelectionPanel.SetActive(false);
        // Lider kartını göster
        foreach (Transform child in leaderCardHolder)
        {
            Destroy(child.gameObject);
        }
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
                cardButton.onClick.AddListener(ShowLeaderSelection);
            }
        }
        // Deste bilgilerini güncelle
        UpdateDeckInfo();
    }
    private void AddCardToDeck(CardStats cardStats)
    {
        // Deste maksimum boyuta ulaştıysa ekleme
        if (playerDeck.Count >= maxDeckSize)
        {
            errorText.text = "Deste maksimum boyuta ulaştı!";
            return;
        }
        // Kartı desteye ekle
        playerDeck.Add(cardStats);
        // Desteyi görsel olarak güncelle
        UpdateDeckVisual();
        // Deste bilgilerini güncelle
        UpdateDeckInfo();
        // Hata metnini temizle
        errorText.text = "";
    }
    private void RemoveCardFromDeck(CardStats cardStats, GameObject cardObj)
    {
        // Kartı desteden çıkar
        playerDeck.Remove(cardStats);
        // Kart objesini yok et
        Destroy(cardObj);
        // Deste bilgilerini güncelle
        UpdateDeckInfo();
    }
    private void UpdateDeckVisual()
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
                    cardButton.onClick.AddListener(() => RemoveCardFromDeck(cardStats, cardObj));
                }
            }
        }
        // ScrollRect'i yeniden ayarla
        DeckBuilderUI deckBuilderUI = GetComponent<DeckBuilderUI>();
        if (deckBuilderUI != null)
        {
            deckBuilderUI.RefreshScrollRects();
        }
    }
    private void UpdateDeckInfo()
    {
        // Deste istatistiklerini hesapla
        int totalCards = playerDeck.Count;
        int unitCards = playerDeck.Count(c => c.cardStatue == CardStatus.Unit);
        int heroCards = playerDeck.Count(c => c.cardStatue == CardStatus.Hero);
        int specialCards = playerDeck.Count(c => c.cardStatue == CardStatus.Special);
        int totalPower = playerDeck.Sum(c => c.cardValue);
        // Deste bilgilerini güncelle
        deckInfoText.text = $"Toplam Kart: {totalCards}/{maxDeckSize}\n" +
                           $"Unit Kart: {unitCards}\n" +
                           $"Hero Kart: {heroCards}\n" +
                           $"Special Kart: {specialCards}\n" +
                           $"Toplam Güç: {totalPower}";
        // Kaydet butonunu güncelle
        saveButton.interactable = totalCards >= minDeckSize && selectedLeader != null;
    }
    private void OnFactionChanged(int index)
    {
        // Faction değiştiğinde filtreleme yap
        string[] factions = { "Tümü", "Northern Realms", "Nilfgaard", "Monsters", "Scoia'tael" };
        currentFaction = factions[index];
        FilterCards();
    }
    private void OnCardTypeChanged(int index)
    {
        // Kart tipi değiştiğinde filtreleme yap
        CardType[] types = { CardType.None, CardType.Melee, CardType.Ranged, CardType.Siege };
        currentCardType = types[index];
        FilterCards();
    }
    // Arama metni ile filtreleme
    public void FilterBySearchText(string searchText)
    {
        currentSearchText = searchText;
        FilterCards();
    }
    private void FilterCards()
    {
        // Kartları filtrele
        filteredCards = new List<CardStats>(allCards);
        // Faction filtresi
        if (currentFaction != "Tümü")
        {
            // Burada faction'a göre filtreleme yapılacak
            // Örnek: filteredCards = filteredCards.Where(c => c.faction == currentFaction).ToList();
        }
        // Kart tipi filtresi
        if (currentCardType != CardType.None)
        {
            filteredCards = filteredCards.Where(c => c.cardType == currentCardType).ToList();
        }
        // Arama metni filtresi
        if (!string.IsNullOrEmpty(currentSearchText))
        {
            filteredCards = filteredCards.Where(c =>
                c.cardName.ToLower().Contains(currentSearchText.ToLower()) ||
                c.cardDesc.ToLower().Contains(currentSearchText.ToLower())
            ).ToList();
        }
        // Filtrelenmiş kartları göster
        ShowCardCollection();
    }
    private void SaveDeck()
    {
        // Deste minimum boyuttan küçükse kaydetme
        if (playerDeck.Count < minDeckSize)
        {
            errorText.text = $"Deste en az {minDeckSize} kart içermelidir!";
            return;
        }
        // Lider seçilmemişse kaydetme
        if (selectedLeader == null)
        {
            errorText.text = "Lütfen bir lider kartı seçin!";
            return;
        }
        // Desteyi kaydet
        // Burada desteyi PlayerPrefs veya başka bir yöntemle kaydedebilirsiniz
        // Örnek: Desteyi JSON olarak kaydet
        SaveDeckToPlayerPrefs();
        // Başarı mesajı göster
        errorText.text = "Deste başarıyla kaydedildi!";
    }
    private void SaveDeckToPlayerPrefs()
    {
        // Kart ID'lerini bir listeye dönüştür
        List<string> deckCardNames = playerDeck.Select(c => c.name).ToList();
        string leaderCardName = selectedLeader.name;
        // JSON'a dönüştür
        string deckJson = JsonUtility.ToJson(new DeckData
        {
            cardNames = deckCardNames.ToArray(),
            leaderCardName = leaderCardName
        });
        // PlayerPrefs'e kaydet
        PlayerPrefs.SetString("PlayerDeck", deckJson);
        PlayerPrefs.Save();
    }
    private void ClearDeck()
    {
        // Desteyi temizle
        playerDeck.Clear();
        // Desteyi görsel olarak güncelle
        UpdateDeckVisual();
        // Deste bilgilerini güncelle
        UpdateDeckInfo();
        // Hata metnini temizle
        errorText.text = "";
    }
    private void GoBack()
    {
        // Ana menüye dön
        // Örnek: SceneManager.LoadScene("MainMenu");
    }
    // Desteyi JSON olarak kaydetmek için yardımcı sınıf
    [System.Serializable]
    private class DeckData
    {
        public string[] cardNames;
        public string leaderCardName;
    }
}
