using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    bool isConnecting;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("NetworkManager initialized");
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
            CreateOrJoinRoom();
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
    public void CreateOrJoinRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (PhotonNetwork.InLobby)
            {
                RoomOptions options = new RoomOptions { MaxPlayers = 2 };
                PhotonNetwork.JoinRandomOrCreateRoom(
                    expectedMaxPlayers: (byte)options.MaxPlayers,
                    roomOptions: options
                );
            }
            else
            {
                Debug.LogWarning("Lobiye bağlı değil!");
            }
        }
        else
        {
            Debug.LogError("Photon bağlantısı hazır değil!");
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance bulunamadı!");
            return;
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
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
        UIController.Instance.UpdateStatus($"Oyuncu Sayısı: {PhotonNetwork.CurrentRoom.PlayerCount}/2");
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError($"Katılamadı: {message}");
        CreateOrJoinRoom();
    }
    public void CreateRoom()
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options);
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }
}