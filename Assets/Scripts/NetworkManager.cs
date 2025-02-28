using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    [SerializeField] byte maxPlayersPerRoom = 2;
    bool isConnecting;
    void Awake()
    {PhotonNetwork.AutomaticallySyncScene = true;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (photonView == null)
        {
            Debug.Log("PhotonView component added");
        }
    }
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
            isConnecting = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            PhotonNetwork.JoinLobby();
            isConnecting = false;
        }
    }
    public override void OnJoinedLobby()
    {
        UIController.Instance.ShowLobbyUI();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        UIController.Instance.ShowConnectionUI();
    }
    public void CreateRoom()
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            EmptyRoomTtl = 10000 // 10 saniye sonra boş oda silinsin
        };
        PhotonNetwork.CreateRoom(null, options);
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Rastgele oda bulunamadı, yeni oda oluşturuluyor...");
        CreateRoom();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance bulunamadı!");
            return;
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
        {
            if (GameManager.Instance.photonView != null)
            {
                GameManager.Instance.photonView.RPC("StartGame", RpcTarget.All);
            }
            else
            {
                Debug.LogError("GameManager PhotonView eksik!");
            }
        }
    }
    public override void OnJoinedRoom()
    {
        StartCoroutine(DelayedUICheck());
    }
    IEnumerator DelayedUICheck()
    {
        yield return new WaitForSeconds(0.5f);
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateStatus($"Odaya katıldı: {PhotonNetwork.CurrentRoom.Name}");
        }
        photonView.RPC("UpdateRoomStatus", RpcTarget.All);
    }
    [PunRPC]
    void UpdateRoomStatus()
    {
        UIController.Instance.UpdateStatus($"Oyuncu Sayısı: {PhotonNetwork.CurrentRoom.PlayerCount}/{maxPlayersPerRoom}");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Oyuncu çıktığında UI'ı güncelle
        photonView.RPC("UpdateRoomStatus", RpcTarget.All);
    }
    public override void OnLeftRoom()
    {
        // Kendi çıkışımızda UI'ı sıfırla
        UIController.Instance.ShowConnectionUI();
    }
}