using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;
    public GameObject boardObject;
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

        // Oyuncuya özel kartlar oluştur
        newDeck.GenerateDeck(playerId);

        return newDeck;
    }


    void AssignBoardRows()
    {
        boardRows = FindObjectsOfType<BoardRow>();
        boardObject = GameObject.Find("GameBoard");
        player1Rows.Clear();
        player2Rows.Clear();

        foreach (var row in boardRows)
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
            boardObject.transform.Rotate(0, 0, 180);

        }
    }


    [PunRPC]
    void CloseUI()
    {
        if (uiObject != null)
        {
            uiObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CloseUI] uiObject referansı bulunamadı!");
        }
    }

    [PunRPC]
    public void PassTurn(int playerId)
    {
        Debug.Log($"[PassTurn] Oyuncu {playerId} pas geçti.");

        if (playerId == 0) player1Passed = true;
        else if (playerId == 1) player2Passed = true;

        if (player1Passed && player2Passed)
        {
            EndRound();
        }
        else
        {
            // Diğer oyuncu zaten pas geçmemişse sırayı değiştir
            int nextPlayer = (playerId + 1) % 2;
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

    // Yeni eklenen yardımcı metot - tek bir yerden sırayı ayarlar
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
        Debug.Log($"[SetTurnRPC] Yeni sıra: Oyuncu {playerId}");
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

        Debug.Log($"[EndTurnRPC] Sıra değiştiriliyor: {previousPlayer} → {nextPlayer}");

        // SetTurnRPC'yi direkt çağırmak yerine SetCurrentTurnPlayer kullanıyoruz
        SetCurrentTurnPlayer(nextPlayer);
    }

    // Bu metot tüm client'ler için çalışmalı
    public void EndTurn()
    {
        // Sadece şu anki sıradaki oyuncu EndTurn yapabilir
        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (localPlayerId == currentTurnPlayer)
        {
            photonView.RPC("EndTurnRPC", RpcTarget.All);
        }
        else
        {
            Debug.LogWarning($"[EndTurn] Sırası olmayan oyuncu ({localPlayerId}) sıra değiştirmeye çalıştı. Şu anki sıra: {currentTurnPlayer}");
        }
    }

    public void PlayCard(int playerId, Card card, BoardRow row)
    {
        Debug.Log($"[PlayCard] Oyuncu {playerId} kart oynuyor. Şu anki sıra: {currentTurnPlayer}");

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
        Debug.Log($"[PlayCardRPC] Oyuncu {playerId} kartı oynadı. Şu an sıra: {currentTurnPlayer}");

        BoardRow targetRow = boardRows[rowIndex];
        Card card = PhotonView.Find(cardViewID)?.GetComponent<Card>();

        if (card == null) return;

        bool success = targetRow.AddCard(card, playerId);
        if (success)
        {
            Debug.Log($"[PlayCardRPC] Oyuncu {playerId} kartı başarıyla oynadı. Sıra değiştiriliyor.");

            // EndTurnRPC'yi hemen çağırmak yerine sıranın değişmesi için bir kare bekleyelim
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(DelayedEndTurn());
            }
        }
        else
        {
            Debug.LogError("[PlayCardRPC] HATA! Kart oynanamadı.");
        }
    }

    System.Collections.IEnumerator DelayedEndTurn()
    {
        yield return null; // Bir frame bekle
        photonView.RPC("EndTurnRPC", RpcTarget.All);
    }

    public void OnPassButtonClicked()
    {
        // Oyuncunun kendi ID'sini alıyoruz
        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        // Sadece sırası gelen oyuncu pas geçebilir
        if (localPlayerId == currentTurnPlayer)
        {
            photonView.RPC("PassTurn", RpcTarget.All, localPlayerId);
        }
        else
        {
            Debug.LogWarning($"[OnPassButtonClicked] Sırası olmayan oyuncu ({localPlayerId}) pas geçmeye çalıştı.");
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