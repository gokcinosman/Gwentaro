using Photon.Pun;
using UnityEngine;
public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;
    private int currentTurn = 0;
    private int[] currentRowPowers = new int[3];
    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    [PunRPC]
    public void StartGame()
    {
        if (photonView.IsMine)
        {
            Debug.Log("Oyun master client tarafından başlatılıyor");
            // Oyun başlangıç mantığı
            DealInitialCards();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed");
        }
    }
    [PunRPC]
    public void ReceiveCards(int cardCount)
    {
        // Kart alma mantığı
        Debug.Log($"{cardCount} kart alındı");
    }
    void DealInitialCards()
    {
        // Her oyuncuya 10 kart dağıt
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ReceiveCards", RpcTarget.All, 10);
        }
    }
    [PunRPC]
    public void UpdateGameState(int rowIndex, int power)
    {
        // Tüm oyuncular arasında senkronize oyun durumu
        currentRowPowers[rowIndex] += power;
        UpdateAllPlayersUI();
    }
    void UpdateAllPlayersUI()
    {
        // Tüm oyuncuların UI'ını güncelle
        photonView.RPC("UpdatePlayerUI", RpcTarget.All);
    }
    void LogNetworkInfo()
    {
        Debug.Log($"Master: {PhotonNetwork.IsMasterClient} | Players: {PhotonNetwork.CurrentRoom.PlayerCount} | Ping: {PhotonNetwork.GetPing()}");
    }
}