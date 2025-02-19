using TMPro;
using UnityEngine.UI;
using UnityEngine;
public class UIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] Button createRoomBtn;
    public static UIController Instance;
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
        createRoomBtn.onClick.AddListener(NetworkManager.Instance.CreateOrJoinRoom);
    }
    public void UpdateStatus(string message)
    {
        statusText.text = message;
    }
}