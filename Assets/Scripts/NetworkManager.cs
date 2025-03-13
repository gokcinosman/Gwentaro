using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    [SerializeField] byte maxPlayersPerRoom = 2;
    bool isConnecting;
    [SerializeField] InputField roomNameInputField; // Oda adı için UI InputField
    private bool isCreatingRoom = false; // Oda oluşturma işlemi devam ediyor mu?
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
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
        MainMenu.Instance.ShowLobbyUI();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        MainMenu.Instance.ShowConnectionUI();
    }
    public void CreateRoom()
    {
        if (isCreatingRoom) return; // Eğer zaten oda oluşturma işlemi devam ediyorsa, tekrar çağrılmasını engelle
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Photon henüz hazır değil! Lütfen bekleyin.");
            return;
        }
        string roomName = roomNameInputField.text; // Kullanıcıdan oda adını al
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("Oda adı boş olamaz!");
            return;
        }
        isCreatingRoom = true; // Oda oluşturma işlemi başladı
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            EmptyRoomTtl = 10000 // 10 saniye sonra boş oda silinsin
        };
        PhotonNetwork.CreateRoom(roomName, options); // Oda adını kullanarak oda oluştur
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Oda oluşturma başarısız: {message}");
        isCreatingRoom = false; // Oda oluşturma işlemi başarısız oldu, flag'i sıfırla
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Oda başarıyla oluşturuldu!");
        isCreatingRoom = false; // Oda oluşturma işlemi başarılı, flag'i sıfırla
    }
    public void JoinRoom()
    {
        string roomName = roomNameInputField.text; // Kullanıcıdan oda adını al
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("Oda adı boş olamaz!");
            return;
        }
        PhotonNetwork.JoinRoom(roomName); // Belirtilen oda adına katıl
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
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
        if (MainMenu.Instance != null)
        {
            MainMenu.Instance.UpdateStatus($"Odaya katıldı: {PhotonNetwork.CurrentRoom.Name}");
        }
        photonView.RPC("UpdateRoomStatus", RpcTarget.All);
    }
    [PunRPC]
    void UpdateRoomStatus()
    {
        MainMenu.Instance.UpdateStatus($"Oyuncu Sayısı: {PhotonNetwork.CurrentRoom.PlayerCount}/{maxPlayersPerRoom}");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Oyuncu çıktığında UI'ı güncelle
        photonView.RPC("UpdateRoomStatus", RpcTarget.All);
    }
    public override void OnLeftRoom()
    {
        // Kendi çıkışımızda UI'ı sıfırla
        MainMenu.Instance.ShowConnectionUI();
    }
}