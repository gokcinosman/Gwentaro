using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;
    public GameObject enemyBoardObject;
    public Deck player1Deck, player2Deck;
    public GameObject uiObject;
    public PlayerManager player1, player2;
    public List<BoardRow> player1Rows = new List<BoardRow>(), player2Rows = new List<BoardRow>();
    public BoardRow[] boardRows;
    private int currentTurnPlayer;
    private int player1Score = 0, player2Score = 0, currentRound = 1;
    private bool gameOver = false, player1Passed = false, player2Passed = false;
    public int CurrentTurnPlayer => currentTurnPlayer;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "CardScene")
        {
            AssignBoardRows();
            AssignDecks();
        }
    }
    void AssignDecks()
    {
        Deck[] allDecks = FindObjectsOfType<Deck>();
        foreach (var deck in allDecks)
        {
            if (deck.ownerPlayerId == 0)
            {
                player1Deck = deck;
            }
            else if (deck.ownerPlayerId == 1)
            {
                player2Deck = deck;
            }
        }
        // Eğer desteler otomatik atanmazsa, her oyuncuya özel oluştur
        if (player1Deck == null)
        {
            player1Deck = CreateDeckForPlayer(0);
        }
        if (player2Deck == null)
        {
            player2Deck = CreateDeckForPlayer(1);
        }
    }
    Deck CreateDeckForPlayer(int playerId)
    {
        GameObject deckObj = new GameObject($"Player{playerId}_Deck");
        Deck newDeck = deckObj.AddComponent<Deck>();
        newDeck.ownerPlayerId = playerId;
        // Eğer bu oyuncunun kaydedilmiş bir destesi varsa, onu yükle
        if (playerId == PhotonNetwork.LocalPlayer.ActorNumber - 1 && PlayerPrefs.HasKey("PlayerDeck"))
        {
            LoadPlayerDeck(newDeck);
        }
        else
        {
            // Kaydedilmiş deste yoksa, rastgele kartlar oluştur
            newDeck.GenerateDeck(playerId);
        }
        return newDeck;
    }
    // Oyuncunun kaydedilmiş destesini yükle
    private void LoadPlayerDeck(Deck deck)
    {
        try
        {
            // PlayerPrefs'ten deste verilerini al
            string deckJson = PlayerPrefs.GetString("PlayerDeck");
            DeckData deckData = JsonUtility.FromJson<DeckData>(deckJson);
            if (deckData != null && deckData.cardNames != null && deckData.cardNames.Length > 0)
            {
                List<CardStats> deckCards = new List<CardStats>();
                foreach (string cardName in deckData.cardNames)
                {
                    CardStats card = Resources.Load<CardStats>($"Cards/{cardName}");
                    if (card != null)
                    {
                        deckCards.Add(card);
                    }
                }
                // Desteyi oluştur
                if (deckCards.Count > 0)
                {
                    // Kaydedilmiş kartlardan deste oluştur
                    deck.GenerateDeckFromCards(deck.ownerPlayerId, deckCards);
                    Debug.Log($"Oyuncu destesi yüklendi: {deckCards.Count} kart");
                }
                else
                {
                    Debug.LogWarning("Kaydedilmiş destede kart bulunamadı, rastgele deste oluşturuluyor.");
                    deck.GenerateDeck(deck.ownerPlayerId);
                }
            }
            else
            {
                Debug.LogWarning("Geçerli deste verisi bulunamadı, rastgele deste oluşturuluyor.");
                deck.GenerateDeck(deck.ownerPlayerId);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Deste yüklenirken hata oluştu: {e.Message}");
            deck.GenerateDeck(deck.ownerPlayerId);
        }
    }
    // Deste verilerini saklamak için yardımcı sınıf
    [System.Serializable]
    private class DeckData
    {
        public string[] cardNames;
    }
    void SwapMeleeAndSiegeParents()
    {
        Transform player1MeleeParent = null, player1SiegeParent = null;
        Transform player2MeleeParent = null, player2SiegeParent = null;
        // BoardRow'ları tarayarak Melee ve Siege olanları bul
        foreach (var row in boardRows)
        {
            if (row.ownerPlayerId == 0) // Player 1 için
            {
                if (row.rowType == CardType.Melee) player1MeleeParent = row.transform.parent;
                if (row.rowType == CardType.Siege) player1SiegeParent = row.transform.parent;
            }
            else if (row.ownerPlayerId == 1) // Player 2 için
            {
                if (row.rowType == CardType.Melee) player2MeleeParent = row.transform.parent;
                if (row.rowType == CardType.Siege) player2SiegeParent = row.transform.parent;
            }
        }
        // Eğer parent'lar bulunduysa sıralarını değiştir
        if (player1MeleeParent != null && player1SiegeParent != null)
        {
            int player1MeleeIndex = player1MeleeParent.GetSiblingIndex();
            int player1SiegeIndex = player1SiegeParent.GetSiblingIndex();
            player1MeleeParent.SetSiblingIndex(player1SiegeIndex);
            player1SiegeParent.SetSiblingIndex(player1MeleeIndex);
        }
        if (player2MeleeParent != null && player2SiegeParent != null)
        {
            int player2MeleeIndex = player2MeleeParent.GetSiblingIndex();
            int player2SiegeIndex = player2SiegeParent.GetSiblingIndex();
            player2MeleeParent.SetSiblingIndex(player2SiegeIndex);
            player2SiegeParent.SetSiblingIndex(player2MeleeIndex);
        }
    }
    void AssignBoardRows()
    {
        boardRows = FindObjectsOfType<BoardRow>();
        enemyBoardObject = GameObject.Find("Board_Player1");
        player1Rows.Clear();
        player2Rows.Clear();
        foreach (var row in boardRows.OrderByDescending(r => r.transform.position.y))
        {
            if (row.ownerPlayerId == 0)
            {
                player1Rows.Add(row);
            }
            else if (row.ownerPlayerId == 1)
            {
                player2Rows.Add(row);
            }
        }
        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (localPlayerId == 1) // Player 2 ise board'u döndür
        {
            enemyBoardObject.transform.SetSiblingIndex(0);
            // Melee ve Siege parent'larını değiştir
            SwapMeleeAndSiegeParents();
        }
    }
    [PunRPC]
    void CloseUI()
    {
        if (uiObject != null)
        {
            uiObject.SetActive(false);
        }
    }
    [PunRPC]
    public void PassTurn(int playerId)
    {
        if (playerId == 0) player1Passed = true;
        else if (playerId == 1) player2Passed = true;
        if (player1Passed && player2Passed)
        {
            EndRound();
        }
        else
        {
            int nextPlayer = (playerId + 1) % 2;
            if ((nextPlayer == 0 && player1Passed) || (nextPlayer == 1 && player2Passed))
            {
                return;
            }
            SetCurrentTurnPlayer(nextPlayer);
        }
    }
    [PunRPC]
    public void StartGame()
    {
        if (photonView.IsMine)
        {
            SetPlayersID();
            AssignBoardRows();
            DetermineFirstPlayer();
            LoadGameScene();
        }
        photonView.RPC("CloseUI", RpcTarget.All);
    }
    void LoadGameScene()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("CardScene");
    }
    void SetPlayersID()
    {
        player1 = new GameObject("Player1").AddComponent<PlayerManager>();
        player1.transform.SetParent(transform);
        player1.playerId = 0;
        player2 = new GameObject("Player2").AddComponent<PlayerManager>();
        player2.transform.SetParent(transform);
        player2.playerId = 1;
    }
    void DetermineFirstPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int firstPlayer = Random.Range(0, 2);
            SetCurrentTurnPlayer(firstPlayer);
        }
    }
    void SetCurrentTurnPlayer(int playerId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SetTurnRPC", RpcTarget.All, playerId);
        }
    }
    [PunRPC]
    public void SetTurnRPC(int playerId)
    {
        currentTurnPlayer = playerId;
        // SyncTurn'ü kaldırdık, sadece UI bilgilendirmesi yapıyoruz
        bool isMyTurn = (PhotonNetwork.LocalPlayer.ActorNumber - 1) == currentTurnPlayer;
        Debug.Log(isMyTurn ? "Senin sıran" : "Rakibin sırası");
    }
    [PunRPC]
    public void EndTurnRPC()
    {
        int previousPlayer = currentTurnPlayer;
        int nextPlayer = (currentTurnPlayer + 1) % 2;
        SetCurrentTurnPlayer(nextPlayer);
    }
    public void EndTurn()
    {
        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (localPlayerId == currentTurnPlayer)
        {
            photonView.RPC("EndTurnRPC", RpcTarget.All);
        }
    }
    public void PlayCard(int playerId, Card card, BoardRow row)
    {
        Debug.Log($"[PlayCard] Oyuncu {playerId} kart oynuyor. Şu anki sıra: {currentTurnPlayer}");
        // Eğer oyuncu pas geçtiyse kart oynayamasın
        if ((playerId == 0 && player1Passed) || (playerId == 1 && player2Passed))
        {
            Debug.LogError($"[PlayCard] HATA! Oyuncu {playerId} pas geçtiği halde kart oynayamaz!");
            return;
        }
        if (playerId != currentTurnPlayer)
        {
            Debug.LogError($"[PlayCard] HATA! Oyuncu {playerId} sırası değilken kart oynuyor!");
            return;
        }
        int rowIndex = System.Array.IndexOf(boardRows, row);
        PhotonView cardPhotonView = card.GetComponent<PhotonView>();
        if (cardPhotonView == null) return;
        photonView.RPC("PlayCardRPC", RpcTarget.All, playerId, cardPhotonView.ViewID, rowIndex);
    }
    [PunRPC]
    void PlayCardRPC(int playerId, int cardViewID, int rowIndex)
    {
        Debug.Log($"[GameManager] PlayCardRPC çağrıldı: playerId={playerId}, cardViewID={cardViewID}, rowIndex={rowIndex}");
        try
        {
            // Kart nesnesini bul
            PhotonView cardView = PhotonView.Find(cardViewID);
            if (cardView == null)
            {
                Debug.LogError($"[GameManager] cardViewID={cardViewID} için PhotonView bulunamadı!");
                return;
            }
            Card card = cardView.GetComponent<Card>();
            if (card == null)
            {
                Debug.LogError($"[GameManager] cardViewID={cardViewID} için Card bileşeni bulunamadı!");
                return;
            }
            // Kart verilerini kontrol et
            if (card.cardStats == null)
            {
                Debug.LogError($"[GameManager] cardViewID={cardViewID} için cardStats null!");
                return;
            }
            // BoardRow kontrolü
            if (boardRows == null)
            {
                Debug.LogError("[GameManager] boardRows null!");
                return;
            }
            if (rowIndex < 0 || rowIndex >= boardRows.Length)
            {
                Debug.LogError($"[GameManager] Geçersiz rowIndex: {rowIndex}, boardRows.Length={boardRows.Length}");
                return;
            }
            BoardRow targetRow = boardRows[rowIndex];
            if (targetRow == null)
            {
                Debug.LogError($"[GameManager] rowIndex={rowIndex} için BoardRow null!");
                return;
            }
            // Kartı sıraya ekle
            Debug.Log($"[GameManager] Kart oynanıyor: {card.name}, Sıra: {rowIndex}, Değer: {card.cardStats.cardValue}");
            targetRow.AddCard(card, playerId);
            // EndTurnRPC'yi hemen çağırmak yerine sıranın değişmesi için bir kare bekleyelim
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(DelayedEndTurn());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] PlayCardRPC işlenirken hata oluştu: {e.Message}\n{e.StackTrace}");
        }
    }
    System.Collections.IEnumerator DelayedEndTurn()
    {
        yield return null; // Bir frame bekle
        photonView.RPC("EndTurnRPC", RpcTarget.All);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !gameOver)
        {
            int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            if (localPlayerId == currentTurnPlayer)
            {
                Debug.Log($"[Update] Oyuncu {localPlayerId} space tuşuyla pas geçti.");
                photonView.RPC("PassTurn", RpcTarget.All, localPlayerId);
            }
        }
    }
    public void EndRound()
    {
        if (gameOver) return;
        int player1Power = CalculateTotalPower(player1Rows);
        int player2Power = CalculateTotalPower(player2Rows);
        if (player1Power > player2Power) player1Score++;
        else if (player2Power > player1Power) player2Score++;
        if (player1Score == 2 || player2Score == 2)
        {
            gameOver = true;
        }
        else
        {
            StartNewRound();
        }
    }
    void StartNewRound()
    {
        currentRound++;
        player1Passed = player2Passed = false;
    }
    int CalculateTotalPower(List<BoardRow> rows)
    {
        int totalPower = 0;
        foreach (var row in rows) totalPower += row.GetTotalPower();
        return totalPower;
    }
}