using TMPro;
using UnityEngine.UI;
using UnityEngine;
public class UIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] Button createRoomBtn;
    [SerializeField] Button joinRandomBtn;
    public static UIController Instance;
    [SerializeField] GameObject connectPanel;
    [SerializeField] GameObject lobbyPanel;
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
    public void ShowLobbyUI()
    {
        connectPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }
    public void ShowConnectionUI()
    {
        lobbyPanel.SetActive(false);
        connectPanel.SetActive(true);
    }
    void Start()
    {
        createRoomBtn.onClick.AddListener(NetworkManager.Instance.CreateRoom);
        joinRandomBtn.onClick.AddListener(NetworkManager.Instance.JoinRandomRoom);
    }
    public void UpdateStatus(string message)
    {
        statusText.text = message;
    }
}