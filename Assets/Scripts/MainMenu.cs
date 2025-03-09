using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviourPunCallbacks
{
    [Header("Ana Menü Panelleri")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject deckBuilderPanel;
    [SerializeField] private GameObject optionsPanel;
    [Header("Ana Menü Butonları")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button deckBuilderButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [Header("Lobi Paneli")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button backFromLobbyButton;
    [SerializeField] private Text roomNameInput;
    [SerializeField] private TMP_InputField playerNameInput;
    [Header("Deste Oluşturma Paneli")]
    [SerializeField] private DeckBuilderUI deckBuilderUI;
    [SerializeField] private Button backFromDeckBuilderButton;
    [Header("Seçenekler Paneli")]
    [SerializeField] private Button backFromOptionsButton;
    [Header("Durum Metinleri")]
    [SerializeField] private TextMeshProUGUI statusText;
    private void Awake()
    {
        // Butonları ayarla
        SetupButtons();
        // Başlangıçta sadece ana menü panelini göster
        ShowMainMenuPanel();
    }
    private void Start()
    {
        // Photon sunucusuna bağlan
        ConnectToPhoton();
        // Oyuncu adını PlayerPrefs'ten yükle
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerNameInput.text = PlayerPrefs.GetString("PlayerName");
        }
    }
    private void SetupButtons()
    {
        // Ana menü butonları
        playButton.onClick.AddListener(OnPlayButtonClicked);
        deckBuilderButton.onClick.AddListener(OnDeckBuilderButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
        // Lobi butonları
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
        backFromLobbyButton.onClick.AddListener(OnBackFromLobbyButtonClicked);
        // Deste oluşturma butonları
        backFromDeckBuilderButton.onClick.AddListener(OnBackFromDeckBuilderButtonClicked);
        // Seçenekler butonları
        backFromOptionsButton.onClick.AddListener(OnBackFromOptionsButtonClicked);
    }
    private void ConnectToPhoton()
    {
        statusText.text = "Sunucuya bağlanıyor...";
        PhotonNetwork.ConnectUsingSettings();
    }
    #region Panel Gösterme/Gizleme
    private void ShowMainMenuPanel()
    {
        mainMenuPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        deckBuilderPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }
    private void ShowLobbyPanel()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        deckBuilderPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }
    private void ShowDeckBuilderPanel()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        deckBuilderPanel.SetActive(true);
        optionsPanel.SetActive(false);
        // DeckBuilderUI'yi etkinleştir
        if (deckBuilderUI != null)
        {
            deckBuilderUI.OpenDeckBuilder();
        }
    }
    private void ShowOptionsPanel()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        deckBuilderPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }
    #endregion
    #region Buton Olayları
    private void OnPlayButtonClicked()
    {
        ShowLobbyPanel();
    }
    private void OnDeckBuilderButtonClicked()
    {
        ShowDeckBuilderPanel();
    }
    private void OnOptionsButtonClicked()
    {
        ShowOptionsPanel();
    }
    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
    private void OnCreateRoomButtonClicked()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            statusText.text = "Lütfen bir oda adı girin!";
            return;
        }
        if (string.IsNullOrEmpty(playerNameInput.text))
        {
            statusText.text = "Lütfen bir oyuncu adı girin!";
            return;
        }
        // Oyuncu adını kaydet
        PhotonNetwork.NickName = playerNameInput.text;
        PlayerPrefs.SetString("PlayerName", playerNameInput.text);
        PlayerPrefs.Save();
        // Oda oluştur
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true
        };
        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
        statusText.text = "Oda oluşturuluyor...";
    }
    private void OnJoinRoomButtonClicked()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            statusText.text = "Lütfen bir oda adı girin!";
            return;
        }
        if (string.IsNullOrEmpty(playerNameInput.text))
        {
            statusText.text = "Lütfen bir oyuncu adı girin!";
            return;
        }
        // Oyuncu adını kaydet
        PhotonNetwork.NickName = playerNameInput.text;
        PlayerPrefs.SetString("PlayerName", playerNameInput.text);
        PlayerPrefs.Save();
        // Odaya katıl
        PhotonNetwork.JoinRoom(roomNameInput.text);
        statusText.text = "Odaya katılınıyor...";
    }
    private void OnBackFromLobbyButtonClicked()
    {
        ShowMainMenuPanel();
    }
    private void OnBackFromDeckBuilderButtonClicked()
    {
        // DeckBuilderUI'yi kapat
        if (deckBuilderUI != null)
        {
            deckBuilderUI.CloseDeckBuilder();
        }
        ShowMainMenuPanel();
    }
    private void OnBackFromOptionsButtonClicked()
    {
        ShowMainMenuPanel();
    }
    #endregion
    #region Photon Callbacks
    public override void OnConnectedToMaster()
    {
        statusText.text = "Sunucuya bağlandı!";
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = $"Bağlantı kesildi: {cause}";
        ConnectToPhoton();
    }
    public override void OnCreatedRoom()
    {
        statusText.text = "Oda oluşturuldu!";
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Oda oluşturma başarısız: {message}";
    }
    public override void OnJoinedRoom()
    {
        statusText.text = "Odaya katıldı!";
        // Eğer oda dolu ise oyunu başlat
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadGameScene();
            }
        }
        else
        {
            statusText.text = "Diğer oyuncu bekleniyor...";
        }
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Odaya katılma başarısız: {message}";
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = $"{newPlayer.NickName} odaya katıldı!";
        // Eğer oda dolu ise oyunu başlat
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadGameScene();
            }
        }
    }
    #endregion
    private void LoadGameScene()
    {
        // Oyun sahnesini yükle
        PhotonNetwork.LoadLevel("CardScene");
    }
}