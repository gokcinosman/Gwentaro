using UnityEngine;
using TMPro;
public class DebugConsole : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI debugText;
    static TextMeshProUGUI _debugText;
    void Awake()
    {
        _debugText = debugText;
        Application.logMessageReceived += HandleLog;
    }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (_debugText != null)
        {
            _debugText.text += $"\n[{System.DateTime.Now:HH:mm:ss}] {logString}";
        }
    }
    void Start()
    {
        Debug.unityLogger.logEnabled = true;
        Debug.Log("Build versiyon: " + Application.version);
    }
}