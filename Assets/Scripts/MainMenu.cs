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
    public static MainMenu Instance;
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
    [Header("Deste Oluşturma Paneli")]
    [SerializeField] private DeckBuilderUI deckBuilderUI;
    [SerializeField] private Button backFromDeckBuilderButton;
    [Header("Seçenekler Paneli")]
    [SerializeField] private Button backFromOptionsButton;
    [Header("Durum Metinleri")]
    [SerializeField] private TextMeshProUGUI statusText;
    [Header("Bağlantı Panelleri")]
    [SerializeField] GameObject connectionPanel;
    [SerializeField] GameObject inGamePanel;
    private void Awake()
    {
        // Singleton pattern'i ekle
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Butonları ayarla
        SetupButtons();
        // Başlangıçta sadece ana menü panelini göster
        ShowMainMenuPanel();
    }
    private bool isLoadingLevel;
    public void OnLevelWasLoaded(int level)
    {
        isLoadingLevel = false;
    }
    private void Start()
    {
        // Başlangıçta butonları devre dışı bırak
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        // Sunucuya bağlan
        PhotonNetwork.ConnectUsingSettings();
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
    public void ShowLobbyUI()
    {
        connectionPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        inGamePanel.SetActive(false);
        UpdateStatus("Lobiye bağlandı");
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
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Sunucuya bağlantı henüz hazır değil. Lütfen bekleyin.");
            return;
        }
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true
        };
        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }
    private void OnJoinRoomButtonClicked()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Sunucuya bağlantı henüz hazır değil. Lütfen bekleyin.");
            return;
        }
        PhotonNetwork.JoinRoom(roomNameInput.text);
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
        Debug.Log("Master sunucuya bağlanıldı!");
        // Butonları aktif hale getir
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
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
        Debug.LogError($"Oda oluşturma başarısız: {message}");
        statusText.text = $"Oda oluşturma başarısız: {message}";
    }
    public override void OnJoinedRoom()
    {
        Debug.Log($"Odaya katılındı: {PhotonNetwork.CurrentRoom.Name}");
        statusText.text = "Odaya katıldı!";
        // Eğer oda dolu ise ve sahne henüz yüklenmiyorsa oyunu başlat
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && !isLoadingLevel)
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
        Debug.LogError($"Odaya katılma başarısız: {message}");
        statusText.text = $"Odaya katılma başarısız: {message}";
    }
    #endregion
    private void LoadGameScene()
    {
        if (PhotonNetwork.IsMasterClient && !isLoadingLevel)
        {
            // Sahne yükleme durumunu işaretle
            isLoadingLevel = true;
            // Tüm oyuncular için sahneyi yükle
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            // Gecikmeli yükleme için
            StartCoroutine(DelayedLoadLevel());
        }
    }
    private IEnumerator DelayedLoadLevel()
    {
        // Kısa bir gecikme ekleyin
        yield return new WaitForSeconds(0.2f);
        PhotonNetwork.LoadLevel("CardScene");
    }
    // UIController'dan taşınan metodlar
    public void ShowConnectionUI()
    {
        connectionPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        inGamePanel.SetActive(false);
    }
    public void ShowInGameUI()
    {
        connectionPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        inGamePanel.SetActive(true);
    }
    public void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}