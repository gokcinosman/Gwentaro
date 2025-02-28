using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;
    public GameObject uiObject;
    private int currentTurnPlayer;
    public int CurrentTurnPlayer => currentTurnPlayer;
    private int[] currentRowPowers = new int[3];
    private int player1Score = 0;
    private int player2Score = 0;
    private int currentRound = 1;
    private bool gameOver = false;
    private bool player1Passed = false;
    private bool player2Passed = false;
    public PlayerManager player1;
    public PlayerManager player2;
    public BoardRow[] player1Rows; // Oyuncu 1'in satırları
    public BoardRow[] player2Rows; // Oyuncu 2'nin satırları

    void AssignRows()
    {
        foreach (var row in player1Rows)
        {
            row.ownerPlayerId = 0;
            Debug.Log($"[AssignRows] Oyuncu 1 için row atandı: {row.name}");
        }
        foreach (var row in player2Rows)
        {
            row.ownerPlayerId = 1;
            Debug.Log($"[AssignRows] Oyuncu 2 için row atandı: {row.name}");
        }
    }
    void PositionBoardRows()
    {
        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? 0 : 1;

        if (localPlayerId == 1) // Player 2 ise board'u çevir
        {
            foreach (var row in player1Rows)
            {
                row.transform.Rotate(0, 0, 180); // 180 derece döndür
            }
            foreach (var row in player2Rows)
            {
                row.transform.Rotate(0, 0, 180); // 180 derece döndür
            }
        }
    }
    [PunRPC]
    public void CloseUI()
    {

        uiObject.SetActive(false);


    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne değiştiğinde GameManager yok olmasın
        }
        else
        {
            Destroy(gameObject); // Eğer zaten bir Instance varsa, ikinci bir GameManager oluşmasını engelle
        }
    }

    [PunRPC]
    public void PassTurn(int playerId)
    {
        if (playerId == 0)
        {
            player1Passed = true;
            Debug.Log("Oyuncu 1 pas geçti!");
        }
        else if (playerId == 1)
        {
            player2Passed = true;
            Debug.Log("Oyuncu 2 pas geçti!");
        }

        // Eğer her iki oyuncu da pas geçtiyse raundu bitir
        if (player1Passed && player2Passed)
        {
            EndRound();
        }
        else
        {
            EndTurn(); // Sıradaki oyuncuya geç
        }
    }


    [PunRPC]
    public void StartGame()
    {
        if (photonView.IsMine)
        {
            SetPlayersID(); // Oyuncuların ID'sini belirle
            AssignRows(); // Satır sahipliklerini belirle
            Debug.Log("Oyun master client tarafından başlatılıyor");
            DealInitialCards();
            DetermineFirstPlayer(); // Kimin başlayacağını belirle
            LoadGameScene();
            PositionBoardRows();
        }
        photonView.RPC("CloseUI", RpcTarget.All);
    }


    void LoadGameScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("CardScene"); // Sadece master client çağırıyor
        }
    }


    public void SetPlayersID()
    {
        Debug.Log("SetPlayersID() çağrıldı.");

        GameObject player1Obj = new GameObject("Player1");
        player1Obj.transform.SetParent(transform); // GameManager’ın altına ekle
        player1 = player1Obj.AddComponent<PlayerManager>();
        player1.playerId = 0;

        GameObject player2Obj = new GameObject("Player2");
        player2Obj.transform.SetParent(transform); // GameManager’ın altına ekle
        player2 = player2Obj.AddComponent<PlayerManager>();
        player2.playerId = 1;
        player1 = player1Obj.GetComponent<PlayerManager>();
        player2 = player2Obj.GetComponent<PlayerManager>();
        Debug.Log($"[SetPlayersID] Player1: {player1}, Player2: {player2}");
    }



    void DealInitialCards()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // photonView.RPC("ReceiveCards", RpcTarget.All, 10);
        }
    }

    void DetermineFirstPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int firstPlayer = Random.Range(0, PhotonNetwork.PlayerList.Length); // Rastgele bir oyuncu seç
            photonView.RPC("SetTurn", RpcTarget.All, firstPlayer); // Tüm oyunculara bildir
        }
    }

    void StartNewRound()
    {
        currentRound++;
        player1Passed = false;
        player2Passed = false;

        Debug.Log($"Yeni raunt başladı! Şu an {currentRound}. raunt");

        for (int i = 0; i < currentRowPowers.Length; i++)
        {
            currentRowPowers[i] = 0;
        }
    }

    [PunRPC]
    public void SetTurnRPC(int playerId)
    {
        currentTurnPlayer = playerId;
        Debug.Log($"[SetTurnRPC] Yeni sıra: Oyuncu {playerId}");
    }

    public void EndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int nextPlayer = (currentTurnPlayer + 1) % 2; // Oyuncular sırayla değişsin
            photonView.RPC("SetTurnRPC", RpcTarget.All, nextPlayer);
        }
    }

    public void EndRound()
    {
        if (gameOver) return;

        int player1Power = CalculateTotalPower(0); // Oyuncu 1’in toplam gücü
        int player2Power = CalculateTotalPower(1); // Oyuncu 2’nin toplam gücü

        if (player1Power > player2Power)
        {
            player1Score++;
            Debug.Log("Oyuncu 1 bu raundu kazandı!");
        }
        else if (player2Power > player1Power)
        {
            player2Score++;
            Debug.Log("Oyuncu 2 bu raundu kazandı!");
        }
        else
        {
            Debug.Log("Bu raund berabere!");
        }

        // Oyunu kazanma kontrolü
        if (player1Score == 2)
        {
            gameOver = true;
            Debug.Log("Oyuncu 1 oyunu kazandı!");
        }
        else if (player2Score == 2)
        {
            gameOver = true;
            Debug.Log("Oyuncu 2 oyunu kazandı!");
        }
        else
        {
            StartNewRound();
        }
    }
    private int CalculateTotalPower(int playerIndex)
    {
        int totalPower = 0;

        // Burada her oyuncunun oynadığı kartların toplam gücünü hesaplayacağız
        // Örneğin her satırın gücünü topluyoruz:
        foreach (int power in currentRowPowers)
        {
            totalPower += power;
        }

        return totalPower;
    }


    [PunRPC]
    public void SetTurn(int playerId)
    {
        currentTurnPlayer = playerId;
        Debug.Log($"Tur sırası: Oyuncu {playerId}");
    }
    public void CheckEndGameCondition()
    {
        bool bothPlayersPassed = player1Passed && player2Passed;
        bool noCardsLeft = CheckIfPlayersHaveNoCards();

        if (bothPlayersPassed || noCardsLeft)
        {
            EndRound();
        }
    }
    private bool CheckIfPlayersHaveNoCards()
    {
        return player1.GetHandCount() == 0 && player2.GetHandCount() == 0;
    }
    [PunRPC]
    public void PlayCardRPC(int playerId, int cardViewID, int rowIndex)
    {
        PhotonView cardPhotonView = PhotonView.Find(cardViewID);

        if (cardPhotonView == null)
        {
            Debug.LogError($"[PlayCardRPC] Kart bulunamadı! ViewID: {cardViewID}. Belki de kart senkronize edilmedi?");
            return;
        }

        Card card = cardPhotonView.GetComponent<Card>();
        if (card == null)
        {
            Debug.LogError($"[PlayCardRPC] Kart bileşeni bulunamadı! ViewID: {cardViewID}");
            return;
        }

        BoardRow row = (playerId == 0) ? GameManager.Instance.player1Rows[rowIndex] : GameManager.Instance.player2Rows[rowIndex];
        if (row == null)
        {
            Debug.LogError($"[PlayCardRPC] Row bulunamadı! Index: {rowIndex}");
            return;
        }

        PlayerManager player = playerId == 0 ? GameManager.Instance.player1 : GameManager.Instance.player2;
        if (player == null)
        {
            Debug.LogError($"[PlayCardRPC] Player bulunamadı! PlayerID: {playerId}");
            return;
        }

        if (player.PlayCardOnRow(card, row))
        {
            Debug.Log($"[PlayCardRPC] Oyuncu {playerId} kart oynadı: {card}");
        }
        else
        {
            Debug.Log("[PlayCardRPC] Kart oynama başarısız!");
        }
    }


    public void PlayCard(int playerId, Card card, BoardRow row)
    {
        if (playerId != currentTurnPlayer) // Eğer sırası değilse kart oynayamasın
        {
            Debug.Log($"[PlayCard] Oyuncu {playerId} sırası değil! Şu an {currentTurnPlayer}'ın sırası.");
            return;
        }

        int rowIndex = (playerId == 0) ? System.Array.IndexOf(player1Rows, row) : System.Array.IndexOf(player2Rows, row);
        PhotonView cardPhotonView = card.GetComponent<PhotonView>();

        if (cardPhotonView == null)
        {
            Debug.LogError("[PlayCard] Kartın PhotonView bileşeni eksik!");
            return;
        }

        int cardViewID = cardPhotonView.ViewID;
        Debug.Log($"[PlayCard] Kart oynanıyor. PlayerID: {playerId}, CardViewID: {cardViewID}, RowIndex: {rowIndex}");

        photonView.RPC("PlayCardRPC", RpcTarget.All, playerId, cardViewID, rowIndex);
        EndTurn(); // Sıradaki oyuncuya geç
    }



    public void OnPassButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int playerId = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? 0 : 1; // Oyuncu ID’sini al
            photonView.RPC("PassTurn", RpcTarget.All, playerId);
        }
    }


}
