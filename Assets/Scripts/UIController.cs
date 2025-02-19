using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{
    public static UIController Instance;
    [Header("Panels")]
    [SerializeField] GameObject connectionPanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject inGamePanel;
    [Header("Buttons")]
    [SerializeField] Button connectBtn;
    [SerializeField] Button createRoomBtn;
    [SerializeField] Button joinRandomBtn;
    [Header("Texts")]
    [SerializeField] TextMeshProUGUI statusText;
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
        }
    }
    void Start()
    {
        connectBtn.onClick.AddListener(NetworkManager.Instance.ConnectToPhoton);
        createRoomBtn.onClick.AddListener(NetworkManager.Instance.CreateRoom);
        joinRandomBtn.onClick.AddListener(NetworkManager.Instance.JoinRandomRoom);
        ShowConnectionUI();
    }
    public void ShowConnectionUI()
    {
        connectionPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        inGamePanel.SetActive(false);
    }
    public void ShowLobbyUI()
    {
        connectionPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        inGamePanel.SetActive(false);
        UpdateStatus("Lobiye bağlandı");
    }
    public void ShowInGameUI()
    {
        connectionPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        inGamePanel.SetActive(true);
    }
    public void UpdateStatus(string message)
    {
        statusText.text = message;
    }
}