using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextUpdater : MonoBehaviour, IObserver
{
    private TextMeshProUGUI textComponent;
    public BoardRow boardRow;
    public bool isTotalScore = false;
    public int playerId;
    private ScoreManager scoreManager;

    void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();

    }

    private void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager == null)
        {
            return;
        }

        if (isTotalScore)
        {
            scoreManager.AddObserver(this);
            textComponent.text = "0";
        }
        else if (boardRow != null)
        {
            boardRow.AddObserver(this);
            textComponent.text = boardRow.GetTotalPower().ToString();
        }
    }

    public void OnNotify(string message, BoardRow source)
    {
        if (textComponent != null)
        {
            if (isTotalScore)
            {
                string[] scores = message.Split(':');
                if (scores.Length == 2)
                {
                    textComponent.text = playerId == 1 ? scores[0] : scores[1];
                }
            }
            else if (source != null && source == boardRow)
            {
                textComponent.text = message;
            }
        }
    }

    private void OnDestroy()
    {
        if (isTotalScore && scoreManager != null)
        {
            scoreManager.RemoveObserver(this);
        }
        else if (boardRow != null)
        {
            boardRow.RemoveObserver(this);
        }
    }

    private void OnValidate()
    {
        if (isTotalScore)
        {
            boardRow = null;
        }
    }
}