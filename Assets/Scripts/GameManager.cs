using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;
    private int currentTurnPlayer; // Şu anki turda olan oyuncunun ID'si
    private int[] currentRowPowers = new int[3];
    private int player1Score = 0;
    private int player2Score = 0;
    private int currentRound = 1;
    private bool gameOver = false;
    private bool player1Passed = false;
    private bool player2Passed = false;


    void Awake()
    {
        if (Instance == null) Instance = this;
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
            Debug.Log("Oyun master client tarafından başlatılıyor");
            DealInitialCards();
            DetermineFirstPlayer(); // Kimin başlayacağını belirle
        }
    }

    void DealInitialCards()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ReceiveCards", RpcTarget.All, 10);
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


    public void EndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int nextPlayer = (currentTurnPlayer + 1) % PhotonNetwork.PlayerList.Length; // Sıradaki oyuncuyu belirle
            photonView.RPC("SetTurn", RpcTarget.All, nextPlayer);
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
        if (/* oyuncuların daha oynayacak kartı kalmadıysa veya tur tamamlandıysa */)
        {
            EndRound();
        }
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
