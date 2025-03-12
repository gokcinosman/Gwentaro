using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [Header("Deste Ayarları")]
    [SerializeField] private int maxDeckSize = 40; // Maksimum deste boyutu
    [SerializeField] private int minDeckSize = 25; // Minimum deste boyutu
    [SerializeField] private GameObject cardPrefab; // Kart prefabı
    [Header("UI Referansı")]
    [SerializeField] private DeckBuilderUI deckUI; // DeckBuilderUI referansı
    // Deste verileri
    private List<CardStats> allCards = new List<CardStats>(); // Tüm kartlar
    private List<CardStats> filteredCards = new List<CardStats>(); // Filtrelenmiş kartlar
    private List<CardStats> playerDeck = new List<CardStats>(); // Oyuncu destesi
    private string currentFaction = "Tümü"; // Şu anki seçili faction
    private CardType currentCardType = CardType.None; // Şu anki seçili kart tipi
    private string currentSearchText = ""; // Şu anki arama metni
    private void Awake()
    {
        // Tüm kartları yükle
        LoadAllCards();
        // UI referansını kontrol et
        if (deckUI == null)
        {
            deckUI = GetComponent<DeckBuilderUI>();
            if (deckUI == null)
            {
                Debug.LogError("[BuildDeck] DeckBuilderUI referansı bulunamadı!");
            }
        }
    }
    private void Start()
    {
        // Filtrelenmiş kartları hazırla
        FilterCards();
        // Başlangıçta kartları oluştur
        if (deckUI != null)
        {
            // Koleksiyon kartlarını oluştur
            deckUI.UpdateCardCollection(filteredCards, cardPrefab, AddCardToDeck);
            // Deste kartlarını oluştur (eğer varsa)
            if (playerDeck.Count > 0)
            {
                deckUI.UpdatePlayerDeck(playerDeck, cardPrefab, RemoveCardFromDeck);
            }
        }
        else
        {
            Debug.LogError("[BuildDeck] DeckBuilderUI referansı bulunamadı! UI güncellenemeyecek.");
        }
        // UI'ı güncelle
        UpdateUI();
    }
    private void LoadAllCards()
    {
        // Tüm CardStats ScriptableObject'lerini Resources klasöründen yükle
        CardStats[] cards = Resources.LoadAll<CardStats>("Cards");
        allCards.AddRange(cards);
        filteredCards = new List<CardStats>(allCards);
        LoadDeckFromPlayerPrefs();
    }
    // Desteyi PlayerPrefs'ten yükle
    private void LoadDeckFromPlayerPrefs()
    {
        // PlayerPrefs'ten desteyi al
        if (PlayerPrefs.HasKey("PlayerDeck"))
        {
            string deckJson = PlayerPrefs.GetString("PlayerDeck");
            DeckData deckData = JsonUtility.FromJson<DeckData>(deckJson);
            // Deste kartlarını yükle
            playerDeck.Clear();
            foreach (string cardName in deckData.cardNames)
            {
                // Kart adına göre CardStats'ı bul
                CardStats cardStats = allCards.Find(c => c.name == cardName);
                if (cardStats != null)
                {
                    playerDeck.Add(cardStats);
                }
                else
                {
                    Debug.LogWarning($"[BuildDeck] Kaydedilmiş destede bulunan kart bulunamadı: {cardName}");
                }
            }
        }
        else
        {
            Debug.Log("[BuildDeck] Kaydedilmiş deste bulunamadı. Yeni deste oluşturulacak.");
        }
    }
    // UI güncellemelerini yönet
    public void UpdateUI()
    {
        if (deckUI != null)
        {
            // Sadece deste bilgilerini güncelle
            deckUI.UpdateDeckInfo(playerDeck, maxDeckSize, minDeckSize);
            // Lider kartı varsa güncelle
        }
    }
    public void AddCardToDeck(CardStats cardStats)
    {
        // Deste maksimum boyuta ulaştıysa ekleme
        if (playerDeck.Count >= maxDeckSize)
        {
            if (deckUI != null)
            {
                deckUI.ShowError($"Deste maksimum boyuta ({maxDeckSize}) ulaştı!");
            }
            return;
        }
        // Eğer kart zaten destede varsa ekleme
        if (playerDeck.Contains(cardStats))
        {
            if (deckUI != null)
            {
                deckUI.ShowError($"Bu kart zaten destede mevcut: {cardStats.name}");
            }
            return;
        }
        // Kartı desteye ekle
        playerDeck.Add(cardStats);
        // UI'ı güncelle
        UpdateUI();
    }
    // Desteden kart çıkar
    public void RemoveCardFromDeck(CardStats cardStats)
    {
        // Kartı desteden çıkar
        playerDeck.Remove(cardStats);
        // UI'ı güncelle
        UpdateUI();
    }
    // Faction değiştiğinde
    public void OnFactionChanged(int index)
    {
        // Faction değiştiğinde filtreleme yap
        string[] factions = { "Tümü", "Northern Realms", "Nilfgaard", "Monsters", "Scoia'tael" };
        currentFaction = factions[index];
        FilterCards();
    }
    // Kart tipi değiştiğinde
    public void OnCardTypeChanged(int index)
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
    // Kartları filtrele
    private void FilterCards()
    {
        // Önce tüm kartları al
        filteredCards = new List<CardStats>(allCards);
        // Kart tipi filtreleme
        if (currentCardType != CardType.None)
        {
            filteredCards = filteredCards.Where(c => c.cardType == currentCardType).ToList();
        }
        // Arama metni filtreleme
        if (!string.IsNullOrEmpty(currentSearchText))
        {
            filteredCards = filteredCards.Where(c =>
                c.cardName.ToLower().Contains(currentSearchText.ToLower()) ||
                c.cardDesc.ToLower().Contains(currentSearchText.ToLower())
            ).ToList();
        }
        // Lider kartlarını koleksiyonda gösterme
        filteredCards = filteredCards.Where(c => c.cardStatue != CardStatus.Leader).ToList();
        filteredCards = filteredCards.Where(c => DeckContainsCard(c) == false).ToList();
        // Koleksiyon kartlarını güncelle
        if (deckUI != null)
        {
            deckUI.UpdateCardCollection(filteredCards, cardPrefab, AddCardToDeck);
        }
    }
    private bool DeckContainsCard(CardStats cardStats)
    {
        return playerDeck.Contains(cardStats);
    }
    // Desteyi kaydet
    public void SaveDeck()
    {
        // Deste minimum boyuttan küçükse kaydetme
        if (playerDeck.Count < minDeckSize)
        {
            if (deckUI != null)
            {
                deckUI.ShowError($"Deste en az {minDeckSize} kart içermelidir!");
            }
            // return;
        }
        SaveDeckToPlayerPrefs();
        // Başarı mesajı göster
        if (deckUI != null)
        {
            deckUI.ShowError("Deste başarıyla kaydedildi!", false);
        }
    }
    // Desteyi PlayerPrefs'e kaydet
    private void SaveDeckToPlayerPrefs()
    {
        // Kart ID'lerini bir listeye dönüştür
        List<string> deckCardNames = playerDeck.Select(c => c.name).ToList();
        string deckJson = JsonUtility.ToJson(new DeckData
        {
            cardNames = deckCardNames.ToArray(),
        });
        // PlayerPrefs'e kaydet
        PlayerPrefs.SetString("PlayerDeck", deckJson);
        PlayerPrefs.Save();
        Debug.Log($"[BuildDeck] Deste başarıyla kaydedildi: {playerDeck.Count} kart,");
    }
    // Desteyi temizle
    public void ClearDeck()
    {
        // Desteyi temizle
        playerDeck.Clear();
        // UI'ı güncelle
        UpdateUI();
        // Hata metnini temizle
        if (deckUI != null)
        {
            deckUI.ClearError();
        }
    }
    // Ana menüye dön
    public void GoBack()
    {
        // Ana menüye dön
        // Örnek: SceneManager.LoadScene("MainMenu");
    }
    // Getter metodları
    public List<CardStats> GetFilteredCards() => filteredCards;
    public List<CardStats> GetPlayerDeck() => playerDeck;
    public int GetMaxDeckSize() => maxDeckSize;
    public int GetMinDeckSize() => minDeckSize;
    public GameObject GetCardPrefab() => cardPrefab;
    // Desteyi JSON olarak kaydetmek için yardımcı sınıf
    [System.Serializable]
    private class DeckData
    {
        public string[] cardNames;
    }
}
