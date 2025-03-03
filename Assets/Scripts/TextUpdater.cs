using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TextUpdater : MonoBehaviour, IObserver
{
    private TextMeshProUGUI textComponent;
    public BoardRow boardRow;

    void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();

    }



    public void OnNotify(string message, BoardRow boardRow)
    {
        if (textComponent != null && boardRow != null && boardRow == this.boardRow)
        {
            textComponent.text = message;
        }
    }

    private void Start()
    {
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddObserver(this);
        }
    }

    private void OnDestroy()
    {
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.RemoveObserver(this);
        }
    }
}